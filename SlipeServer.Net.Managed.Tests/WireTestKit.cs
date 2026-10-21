using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using SlipeServer.Net.Managed;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// One record of an <c>mtanet-capture/1</c> file. The on-disk grammar is
/// <c>{seq} {dir} {utc} {endpoint} {bytes} {hex} | {classification}</c>
/// (<see cref="PacketCapture"/> writes it).
/// </summary>
internal sealed record CaptureLine(
    int Seq,
    string Direction,
    string EndPoint,
    int Length,
    byte[] Bytes,
    string Classification)
{
    internal bool IsRx => this.Direction == "RX";

    internal bool IsTx => this.Direction == "TX";

    /// <summary>
    /// The packet tokens <see cref="CaptureDescribe.DescribeDatagram"/> appended to the line, e.g.
    /// <c>wire=0x83 seq=9 rel=3 ch=0 oi=4 bits=224 bytes=28</c>. These are an independent record of
    /// what the wire carried, written by the capture layer from the parsed datagram.
    /// </summary>
    internal IReadOnlyList<PacketToken> Packets => WireTestKit.ParsePackets(this.Classification);
}

/// <summary>One <c>[pN wire=.. seq=.. rel=.. ch=.. oi=.. bits=.. bytes=..]</c> token from a capture line.</summary>
internal sealed record PacketToken(
    byte WireId,
    uint Sequence,
    int Reliability,
    int Channel,
    uint OrderingIndex,
    int BitCount,
    int ByteCount);

/// <summary>
/// Shared plumbing for the wire-level tests: hex literals, capture-file parsing, and a headless
/// <see cref="MtaNetWrapper"/> harness.
///
/// The harness deliberately never calls <see cref="MtaNetWrapper.Start"/>. <c>InjectDatagram</c> is
/// the documented single entry point for one received datagram and works without a socket, and
/// <c>SendRaw</c> records to <see cref="PacketCapture"/> before touching the (null) socket, so a
/// never-started wrapper still produces its complete outbound byte stream. That keeps every test
/// here deterministic, port-free and fast.
/// </summary>
internal static class WireTestKit
{
    /// <summary>
    /// Parses a hex literal. Whitespace is ignored so a vector can be wrapped and annotated.
    /// </summary>
    internal static byte[] Hex(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);
        string cleaned = new(hex.Where(c => !char.IsWhiteSpace(c)).ToArray());
        return Convert.FromHexString(cleaned);
    }

    internal static string HexOf(byte[] bytes) => Convert.ToHexString(bytes);

    internal static IPEndPoint Endpoint(string text)
    {
        int colon = text.LastIndexOf(':');
        return new IPEndPoint(
            IPAddress.Parse(text[..colon]),
            int.Parse(text[(colon + 1)..], CultureInfo.InvariantCulture));
    }

    internal static string EndpointOf(IPEndPoint endpoint) => $"{endpoint.Address}:{endpoint.Port}";

    /// <summary>Parses an <c>mtanet-capture/1</c> file into records, skipping comment lines.</summary>
    internal static IReadOnlyList<CaptureLine> ReadCapture(string path)
    {
        // The writer keeps the file open with FileShare.Read, so a reader must also share writes or
        // the open fails. AutoFlush is on, so everything recorded so far is already on disk.
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);

        var result = new List<CaptureLine>();
        while (reader.ReadLine() is { } line)
        {
            if (line.Length == 0 || line[0] == '#')
                continue;

            int bar = line.IndexOf(" | ", StringComparison.Ordinal);
            string head = bar < 0 ? line : line[..bar];
            string classification = bar < 0 ? string.Empty : line[(bar + 3)..];

            string[] fields = head.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 6)
                continue;

            result.Add(new CaptureLine(
                int.Parse(fields[0], CultureInfo.InvariantCulture),
                fields[1],
                fields[3],
                int.Parse(fields[4], CultureInfo.InvariantCulture),
                Convert.FromHexString(fields[5]),
                classification));
        }

        return result;
    }

    /// <summary>
    /// The repository's <c>capture.log</c>: two recorded real MTA:SA client sessions and the only
    /// wire oracle in the tree. Copied next to the test assembly by the test project.
    /// </summary>
    internal static string CaptureLogPath => Path.Combine(AppContext.BaseDirectory, "capture.log");

    internal static IReadOnlyList<CaptureLine> CaptureLog => ReadCapture(CaptureLogPath);

    internal static CaptureLine CaptureLogRecord(int seq) =>
        CaptureLog.Single(line => line.Seq == seq);

    /// <summary>
    /// Extracts the <c>[pN wire=.. ...]</c> tokens from a classification string.
    /// </summary>
    internal static IReadOnlyList<PacketToken> ParsePackets(string classification)
    {
        var tokens = new List<PacketToken>();
        int index = 0;
        while (true)
        {
            int open = classification.IndexOf("[p", index, StringComparison.Ordinal);
            if (open < 0)
                break;

            int close = classification.IndexOf(']', open);
            if (close < 0)
                break;

            string body = classification[(open + 1)..close];
            index = close + 1;

            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string part in body.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                int equals = part.IndexOf('=');
                if (equals <= 0)
                    continue;
                values[part[..equals]] = part[(equals + 1)..];
            }

            if (!values.TryGetValue("wire", out string? wire))
                continue;

            tokens.Add(new PacketToken(
                byte.Parse(wire.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                uint.Parse(values["seq"], CultureInfo.InvariantCulture),
                int.Parse(values["rel"], CultureInfo.InvariantCulture),
                int.Parse(values["ch"], CultureInfo.InvariantCulture),
                uint.Parse(values["oi"], CultureInfo.InvariantCulture),
                int.Parse(values["bits"], CultureInfo.InvariantCulture),
                int.Parse(values["bytes"], CultureInfo.InvariantCulture)));
        }

        return tokens;
    }
}

/// <summary>One <c>PacketReceived</c> invocation, captured verbatim so every element can be asserted.</summary>
internal sealed record Received(INetWrapper Sender, ulong Address, PacketId Id, byte[] Data, uint? Ping);

/// <summary>
/// A headless wrapper plus its capture file. See <see cref="WireTestKit"/> for why no socket is
/// involved.
/// </summary>
internal sealed class WrapperHarness : IDisposable
{
    /// <summary>
    /// The endpoint the recorded sessions in <c>capture.log</c> used, read from the file rather
    /// than hard-coded. Reusing it means the binary addresses in these tests are the ones the real
    /// client produced.
    /// </summary>
    internal static readonly IPEndPoint CaptureLogPeer =
        WireTestKit.Endpoint(WireTestKit.CaptureLog[0].EndPoint);

    private readonly string directory;
    private bool disposed;

    internal WrapperHarness(
        ushort port = 50666,
        string host = "0.0.0.0",
        int playerCount = 1024,
        IMonotonicClock? clock = null,
        IPeerVersionHints? peerVersionHints = null)
    {
        this.directory = Path.Combine(Path.GetTempPath(), "mtanet-tests", Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(this.directory);
        this.CapturePath = Path.Combine(this.directory, "capture.log");

        this.Wrapper = new MtaNetWrapper(port, host, playerCount, clock: clock, peerVersionHints: peerVersionHints);
        this.Capture = new PacketCapture(this.CapturePath);
        this.Wrapper.Capture = this.Capture;
        this.Wrapper.Trace = this.Trace.Add;
        this.Wrapper.PacketReceived += (sender, address, id, data, ping) =>
            this.Received.Add(new Received(sender, address, id, data, ping));
    }

    internal MtaNetWrapper Wrapper { get; }

    internal PacketCapture Capture { get; }

    internal string CapturePath { get; }

    internal List<Received> Received { get; } = new();

    internal List<string> Trace { get; } = new();

    internal ulong Address => MtaNetWrapper.BinaryAddressOf(CaptureLogPeer);

    /// <summary>Every capture record written so far, both directions, in write order.</summary>
    internal IReadOnlyList<CaptureLine> Lines => WireTestKit.ReadCapture(this.CapturePath);

    internal IReadOnlyList<CaptureLine> Tx => this.Lines.Where(line => line.IsTx).ToList();

    internal IReadOnlyList<CaptureLine> Rx => this.Lines.Where(line => line.IsRx).ToList();

    /// <summary>
    /// Feeds one datagram at the transport boundary, exactly as the receive loop does, after declaring
    /// the <see cref="PrimeOrdering"/> precondition. Use <see cref="InjectRaw"/> from a test that is
    /// itself about ordering.
    /// </summary>
    internal void Inject(byte[] datagram, IPEndPoint? remote = null)
    {
        IPEndPoint peer = remote ?? CaptureLogPeer;
        PrimeOrdering(this.Wrapper, datagram, peer);
        this.Wrapper.InjectDatagram(datagram, datagram.Length, peer);
    }

    internal void InjectHex(string hex, IPEndPoint? remote = null) => this.Inject(WireTestKit.Hex(hex), remote);

    /// <summary>
    /// Feeds one datagram without the <see cref="PrimeOrdering"/> precondition, so the reliability
    /// layer sees the ordering indexes exactly as they arrived. This is the only honest way to test
    /// reordering, holding and dropping.
    /// </summary>
    internal void InjectRaw(byte[] datagram, IPEndPoint? remote = null)
    {
        IPEndPoint peer = remote ?? CaptureLogPeer;
        this.Wrapper.InjectDatagram(datagram, datagram.Length, peer);
    }

    internal void InjectRawHex(string hex, IPEndPoint? remote = null) =>
        this.InjectRaw(WireTestKit.Hex(hex), remote);

    /// <summary>
    /// A recorded vector is a slice out of a live session, not a stream from index 0: <c>capture.log</c>'s
    /// <c>RpcPacketWire83</c> is <c>oi=4</c> on channel 0, while a fresh session expects 0, so the
    /// reliability layer would hold it behind a hole the test never fills. Raising each channel's
    /// expected index to the first index the slice carries stands in for the packets the recording had
    /// already delivered. It only ever moves forward, so it cannot rewind state or mask a drop.
    /// </summary>
    internal static void PrimeOrdering(MtaNetWrapper wrapper, byte[] datagram, IPEndPoint peer)
    {
        ulong binaryAddress = MtaNetWrapper.BinaryAddressOf(peer);
        MtaNetSession? session = wrapper.FindSession(binaryAddress);

        // An offline handshake packet is not a RakNet datagram: its first byte is a route id, and
        // parsing it as a datagram would read an ordering index out of the payload.
        if (OfflineHandshake.Route(datagram, datagram.Length, session is not null) != OfflineRoute.Connected)
            return;

        if (session is null || !RakNetDatagram.TryParse(datagram, datagram.Length, out RakNetDatagram parsed, out _))
            return;

        var ordered = new uint[InboundOrdering.MaxOrderingChannels];
        var sequenced = new uint[InboundOrdering.MaxOrderingChannels];
        Array.Fill(ordered, uint.MaxValue);
        Array.Fill(sequenced, uint.MaxValue);

        foreach (RakNetPacketRecord record in parsed.Packets)
        {
            var reliability = (WireReliability)record.Reliability;
            if (!reliability.CarriesOrdering() || record.OrderingChannel >= InboundOrdering.MaxOrderingChannels)
                continue;

            uint[] first = reliability == WireReliability.ReliableOrdered ? ordered : sequenced;
            first[record.OrderingChannel] = Math.Min(first[record.OrderingChannel], record.OrderingIndex);
        }

        for (byte channel = 0; channel < InboundOrdering.MaxOrderingChannels; channel++)
        {
            if (ordered[channel] == uint.MaxValue && sequenced[channel] == uint.MaxValue)
                continue;

            session.InboundOrdering.Prime(
                channel,
                ordered[channel] == uint.MaxValue ? 0u : ordered[channel],
                sequenced[channel] == uint.MaxValue ? 0u : sequenced[channel]);
        }
    }

    /// <summary>
    /// Injects the recorded real-client <c>RID_OPEN_CONNECTION_REQUEST</c> and clears the event log,
    /// leaving a session record that is <c>HandshakeComplete</c> and <c>IsEstablished</c>.
    /// </summary>
    internal void OpenSession(IPEndPoint? remote = null)
    {
        this.InjectHex(CaptureVectors.OpenConnectionRequest, remote);
        this.Received.Clear();
    }

    /// <summary>
    /// Injects the recorded real-client connected <c>RID_CONNECTION_REQUEST</c> on top of
    /// <see cref="OpenSession"/>, so the peer is fully connected.
    /// </summary>
    internal void ConnectSession(IPEndPoint? remote = null)
    {
        this.OpenSession(remote);
        this.InjectHex(CaptureVectors.ConnectedConnectionRequest, remote);
        this.Received.Clear();
    }

    public void Dispose()
    {
        if (this.disposed)
            return;

        this.disposed = true;

        this.Wrapper.Dispose();
        this.Capture.Dispose();

        try
        {
            Directory.Delete(this.directory, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
