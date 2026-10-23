using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using SlipeServer.Net.Managed;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets.Enums;
using Xunit;
using Xunit.Abstractions;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Opt-in probe for the two inbound reliability-layer behaviours: split reassembly and ordering.
/// Runs only when <c>MTANET_PROBE=1</c> is set.
///
/// <para>
/// Both probes are <b>conformance</b> probes. The split probe asserts that the port reassembles the
/// parts into one whole delivery. The ordering probe asserts that the port delivers in ordering-index
/// order rather than arrival order: index 1 arrives first and is held until index 0 lands.
/// </para>
///
/// <para>
/// These are end-to-end probes over a real UDP socket, which is why they are opt-in; the same
/// contracts are pinned by default in <see cref="InboundOrderingTests"/> and
/// <see cref="SplitReassemblyTests"/>.
/// </para>
///
/// <para>
/// Every datagram is built here from the documented field order, and each one is fed back
/// through <see cref="RakNetDatagram.TryParse"/> before it is sent, so the probe cannot silently test
/// a malformed datagram. The handshake is the literal <see cref="CaptureVectors.OpenRequestBytes"/>
/// transcribed from <c>capture.log</c> seq 1. Binds port 0 and never a fixed port.
/// </para>
/// </summary>
public sealed class TransportGapProbeTests
{
    // wire id 0x85 -> mtaId 0x22, which MtaProtocol.IsWhitelisted admits for any payload length.
    // Every part of a split payload therefore begins with a byte that passes admission, which is
    // what makes the "each part is delivered separately" outcome deterministic rather than a
    // side effect of whichever byte happens to fall on a part boundary.
    private const byte ProbeWireId = 0x85;

    private readonly ITestOutputHelper output;

    public TransportGapProbeTests(ITestOutputHelper output) => this.output = output;

    [ProbeFact]
    public void ProbeInboundSplitReassembly()
    {
        using var probe = new Probe(this.output);

        // 4001 bytes is four times the 1492-byte per-datagram ceiling, so a conforming peer must
        // split it; 1390 bytes per part keeps every part under that ceiling with room to spare.
        const int LogicalLength = 4001;
        const int ChunkBytes = 1390;

        byte[] logical = new byte[LogicalLength];
        Array.Fill(logical, ProbeWireId);

        int partCount = (LogicalLength + ChunkBytes - 1) / ChunkBytes;
        var parts = new List<byte[]>(partCount);
        for (int i = 0; i < partCount; i++)
        {
            int offset = i * ChunkBytes;
            int length = Math.Min(ChunkBytes, LogicalLength - offset);
            byte[] part = ProbeEncoder.Datagram(new ProbePacket
            {
                Sequence = (uint)(10 + i),
                Reliability = 3,

                // Every part of one split message carries that message's ordering index, and this
                // probe's session is a fresh one whose channel 0 expects index 0. A higher index
                // would be held behind a hole the probe never fills, which would make this a test
                // of ordering rather than of split reassembly.
                OrderingIndex = 0,
                Payload = logical.AsSpan(offset, length).ToArray(),
                SplitId = 0x1234,
                SplitIndex = (uint)i,
                SplitCount = (uint)partCount,
            });

            // Self-check: the bytes must parse back as the split part they claim to be.
            Assert.True(RakNetDatagram.TryParse(part, part.Length, out RakNetDatagram parsed, out string error), error);
            Assert.Single(parsed.Packets);
            RakNetPacketRecord record = parsed.Packets[0];
            Assert.True(record.HasSplitData);
            Assert.Equal(0x1234, record.SplitPacketId);
            Assert.Equal((uint)i, record.SplitPacketIndex);
            Assert.Equal((uint)partCount, record.SplitPacketCount);
            this.output.WriteLine(
                $"[self-check] part {i}: datagram={part.Length}B payload={record.Payload.Length}B bitCount={record.BitCount}");

            parts.Add(part);
        }

        this.output.WriteLine(
            $"sending one {LogicalLength}-byte logical message as {partCount} parts " +
            $"({string.Join(", ", parts.ConvertAll(p => $"{p.Length}B"))})");

        probe.Expect(1);
        probe.Send(parts);
        probe.WaitForDeliveries();

        IReadOnlyList<byte[]> deliveries = probe.Deliveries;
        foreach (byte[] delivery in deliveries)
        {
            this.output.WriteLine($"  PacketReceived: {delivery.Length}B");
        }

        // Reassembly happens before delivery, so the four parts raise exactly one event.
        Assert.Single(deliveries);

        // MtaNetWrapper strips the leading wire-id byte once, from the whole message, rather than
        // once per part.
        Assert.Equal(LogicalLength - 1, deliveries[0].Length);
        Assert.Equal(ProbeWireId, deliveries[0][0]);
        Assert.All(deliveries[0], b => Assert.Equal(ProbeWireId, b));

        this.output.WriteLine(
            $"VERDICT: the port reassembled {partCount} parts into one {deliveries[0].Length}-byte " +
            "PacketReceived.");
    }

    [ProbeFact]
    public void ProbeInboundReordering()
    {
        using var probe = new Probe(this.output);

        // Two RELIABLE_ORDERED packets on channel 0, delivered with the ordering index inverted:
        // index 1 arrives before index 0. A conforming receiver buffers index 1 until index 0 lands
        // and then delivers 0, 1.
        byte[] indexOne = ProbeEncoder.Datagram(new ProbePacket
        {
            Sequence = 20,
            Reliability = 3,
            OrderingIndex = 1,
            Payload = new[] { ProbeWireId, (byte)'1' },
        });

        byte[] indexZero = ProbeEncoder.Datagram(new ProbePacket
        {
            Sequence = 21,
            Reliability = 3,
            OrderingIndex = 0,
            Payload = new[] { ProbeWireId, (byte)'0' },
        });

        Assert.True(RakNetDatagram.TryParse(indexOne, indexOne.Length, out RakNetDatagram one, out _));
        Assert.Equal(1u, one.Packets[0].OrderingIndex);
        Assert.True(RakNetDatagram.TryParse(indexZero, indexZero.Length, out RakNetDatagram zero, out _));
        Assert.Equal(0u, zero.Packets[0].OrderingIndex);

        this.output.WriteLine("sending ordering index 1, then ordering index 0, on channel 0");

        probe.Expect(2);
        probe.Send(new[] { indexOne, indexZero });
        probe.WaitForDeliveries();

        IReadOnlyList<byte[]> deliveries = probe.Deliveries;
        foreach (byte[] delivery in deliveries)
        {
            this.output.WriteLine($"  PacketReceived: {delivery.Length}B marker={(char)delivery[0]}");
        }

        Assert.Equal(2, deliveries.Count);
        Assert.Equal((byte)'0', deliveries[0][0]);
        Assert.Equal((byte)'1', deliveries[1][0]);

        this.output.WriteLine(
            "VERDICT: the port held ordering index 1 until index 0 landed and then delivered " +
            "0, 1 — ordering-index order, not arrival order.");
    }

    /// <summary>One inbound datagram's worth of state: a started wrapper and a real UDP peer.</summary>
    private sealed class Probe : IDisposable
    {
        private static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(2);

        private readonly MtaNetWrapper wrapper;
        private readonly UdpClient client;
        private readonly IPEndPoint server;
        private readonly List<byte[]> deliveries = new();
        private readonly ManualResetEventSlim settled = new(false);
        private int expected;

        internal Probe(ITestOutputHelper output)
        {
            this.wrapper = new MtaNetWrapper(port: 0, host: "127.0.0.1")
            {
                Trace = line => output.WriteLine($"[net] {line}"),
            };
            this.wrapper.PacketReceived += this.OnPacketReceived;
            this.wrapper.Start();

            this.server = new IPEndPoint(
                IPAddress.Loopback,
                ((IPEndPoint)BoundSocket(this.wrapper).LocalEndPoint!).Port);
            this.client = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));

            // The literal capture.log seq 1 open request is what creates the session record that
            // HandleConnected requires before it will look at a game packet.
            this.Send(new[] { CaptureVectors.OpenRequestBytes });
        }

        internal IReadOnlyList<byte[]> Deliveries
        {
            get
            {
                lock (this.deliveries)
                    return this.deliveries.ToArray();
            }
        }

        internal void Expect(int count) => this.expected = count;

        internal void Send(IReadOnlyList<byte[]> datagrams)
        {
            foreach (byte[] datagram in datagrams)
                this.client.Send(datagram, datagram.Length, this.server);
        }

        internal void WaitForDeliveries() => this.settled.Wait(SettleTimeout);

        private void OnPacketReceived(INetWrapper _, ulong __, PacketId packetId, byte[] data, uint? ___)
        {
            lock (this.deliveries)
            {
                this.deliveries.Add(data);
                if (this.deliveries.Count >= this.expected)
                    this.settled.Set();
            }
        }

        private static Socket BoundSocket(MtaNetWrapper wrapper)
        {
            const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo field = typeof(MtaNetWrapper).GetField("socket", Flags)
                ?? throw new InvalidOperationException("MtaNetWrapper.socket is missing.");
            return (Socket?)field.GetValue(wrapper)
                ?? throw new InvalidOperationException("MtaNetWrapper is not started.");
        }

        public void Dispose()
        {
            this.client.Dispose();
            this.wrapper.PacketReceived -= this.OnPacketReceived;
            this.wrapper.Stop();
            this.wrapper.Dispose();
            this.settled.Dispose();
        }
    }

    /// <summary>
    /// A datagram encoder written from the documented field order (datagram header, then packet
    /// record). It shares only <see cref="BitStream"/>, the bit primitive; the
    /// packet layout is restated here so the probe is not a round-trip through
    /// <see cref="RakNetDatagramWriter"/>.
    /// </summary>
    private static class ProbeEncoder
    {
        internal static byte[] Datagram(ProbePacket packet)
        {
            var bs = new BitStream(1600);

            bs.WriteBit(false);   // no ack block
            bs.WriteBit(false);   // no second optional 64-bit field

            bs.Write(packet.Sequence);
            bs.WriteBits(new[] { (byte)(packet.Reliability & 0x07) }, 3, true);

            if (packet.Reliability is 1 or 3 or 4)
            {
                bs.WriteBits(new[] { (byte)(packet.OrderingChannel & 0x1F) }, 5, true);
                bs.Write(packet.OrderingIndex);
            }

            if (packet.SplitCount != 0)
            {
                bs.WriteBit(true);
                bs.Write(packet.SplitId);
                bs.WriteCompressed(packet.SplitIndex);
                bs.WriteCompressed(packet.SplitCount);
            }
            else
            {
                bs.WriteBit(false);
            }

            bs.WriteCompressed((ushort)(packet.Payload.Length * 8));
            bs.WriteAlignedBytes(packet.Payload, packet.Payload.Length);
            return bs.ToArray();
        }
    }

    private readonly struct ProbePacket
    {
        internal required uint Sequence { get; init; }

        internal required byte Reliability { get; init; }

        internal byte OrderingChannel { get; init; }

        internal uint OrderingIndex { get; init; }

        internal required byte[] Payload { get; init; }

        internal ushort SplitId { get; init; }

        internal uint SplitIndex { get; init; }

        internal uint SplitCount { get; init; }
    }
}

/// <summary>
/// <see cref="FactAttribute"/> that skips unless <c>MTANET_PROBE=1</c>, matching
/// <see cref="SocketFactAttribute"/>: xunit 2.x has no dynamic skip.
/// </summary>
internal sealed class ProbeFactAttribute : FactAttribute
{
    internal const string GateVariable = "MTANET_PROBE";

    public ProbeFactAttribute()
    {
        if (Environment.GetEnvironmentVariable(GateVariable) != "1")
            this.Skip = $"Opt-in transport-gap probe; set {GateVariable}=1 to run it.";
    }
}
