using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using SlipeServer.Net.Managed;
using SlipeServer.Packets.Enums;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Opt-in socket tests for <see cref="MtaNetWrapper.Start"/>. The default suite is socket-free and
/// deterministic; these tests exercise the real UDP bind and receive path, so they only run when
/// <c>MTANET_SOCKET_TESTS=1</c> is set.
///
/// <para>
/// Every test binds port 0 (never a fixed port) and asserts the bound port, not the configured one.
/// The only input datagram is <see cref="CaptureVectors.OpenRequestBytes"/>, the literal transcribed
/// from <c>capture.log</c> seq 1; nothing here is produced by the port's own writer.
/// </para>
/// </summary>
public sealed class MtaNetWrapperSocketTests
{
    [SocketFact]
    public void SocketStartOnPortZeroAdvertisesTheBoundPortAndServesARealDatagram()
    {
        using var wrapper = new MtaNetWrapper(port: 0, host: "127.0.0.1");
        wrapper.Start();
        try
        {
            int boundPort = ((IPEndPoint)BoundSocket(wrapper).LocalEndPoint!).Port;
            Assert.NotEqual(0, boundPort);

            ulong? advertised = LocalBinaryAddress(wrapper);
            Assert.True(advertised.HasValue);
            Assert.Equal((ulong)boundPort, advertised!.Value & 0xFFFF);
            Assert.Equal(
                MtaNetWrapper.BinaryAddressOf(new IPEndPoint(IPAddress.Loopback, 0)) >> 32,
                advertised.Value >> 32);

            using var client = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            client.Client.ReceiveTimeout = 2000;

            byte[] request = CaptureVectors.OpenRequestBytes;
            client.Send(request, request.Length, new IPEndPoint(IPAddress.Loopback, boundPort));

            var from = new IPEndPoint(IPAddress.Any, 0);
            byte[] reply = client.Receive(ref from);

            Assert.Equal(33, reply.Length);
            Assert.Equal(0x0A, reply[0]);
            Assert.Equal("00FF00FFFEFEFEFEFDFDFDFD78563417", WireTestKit.HexOf(reply[1..17]));
            Assert.Equal(WireTestKit.HexOf(wrapper.ServerGuid), WireTestKit.HexOf(reply[17..33]));
            Assert.Equal(1, wrapper.SessionCount);
        }
        finally
        {
            wrapper.Stop();
        }
    }

    [SocketFact]
    public void SocketStopReleasesTheBoundPortForARestart()
    {
        using var first = new MtaNetWrapper(port: 0, host: "127.0.0.1");
        first.Start();

        int boundPort;
        try
        {
            boundPort = ((IPEndPoint)BoundSocket(first).LocalEndPoint!).Port;
            Assert.NotEqual(0, boundPort);
        }
        finally
        {
            first.Stop();
        }

        // The port is the ephemeral port the first bind observed, not a fixed one. A successful
        // rebind proves Stop() closed the socket rather than leaving it bound.
        using var second = new MtaNetWrapper(port: (ushort)boundPort, host: "127.0.0.1");
        second.Start();
        try
        {
            Assert.Equal(boundPort, ((IPEndPoint)BoundSocket(second).LocalEndPoint!).Port);
            Assert.Equal((ulong)boundPort, LocalBinaryAddress(second)!.Value & 0xFFFF);
        }
        finally
        {
            second.Stop();
        }
    }

    /// <summary>
    /// The dead-peer acceptance scenario, at the transport boundary: two real UDP peers, one of which
    /// stops sending. The silent peer must be retired with exactly one
    /// <c>PACKET_ID_PLAYER_TIMEOUT</c> about one timeout after its last datagram, while the peer that
    /// keeps sending survives the same window and stays in the table.
    ///
    /// <para>
    /// Wall-clock by necessity, hence opt-in like the rest of this class: the sweep compares
    /// <see cref="SessionTimeoutPolicy.TimeoutMilliseconds"/> against a real monotonic clock, so the
    /// boundary cannot be reached without actually waiting. The deterministic counterpart is
    /// <c>SessionTimeoutTests</c>, which advances a manual clock. Both exist on purpose — one pins the
    /// arithmetic, this one proves the arithmetic is wired to a live socket and a live receive loop.
    /// </para>
    ///
    /// <para>
    /// The chatty peer keeps sending across the whole window even though its datagrams are repeats of
    /// the open request and are answered with silence by the "already-connected" arm. That is the
    /// point: liveness is "a datagram arrived", not "a packet was delivered".
    /// </para>
    /// </summary>
    [SocketFact]
    public void ASilentPeerIsRetiredOverRealUdpWhileTheChattyPeerSurvives()
    {
        using var wrapper = new MtaNetWrapper(port: 0, host: "127.0.0.1");
        var clock = Stopwatch.StartNew();
        var gate = new object();
        var notifications = new List<(ulong Address, PacketId Id, byte[] Data, long ElapsedMilliseconds)>();
        wrapper.PacketReceived += (sender, address, id, data, ping) =>
        {
            lock (gate)
                notifications.Add((address, id, data, clock.ElapsedMilliseconds));
        };

        wrapper.Start();
        try
        {
            int boundPort = ((IPEndPoint)BoundSocket(wrapper).LocalEndPoint!).Port;
            var server = new IPEndPoint(IPAddress.Loopback, boundPort);

            using var chatty = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            using var silent = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));

            ulong chattyAddress = MtaNetWrapper.BinaryAddressOf((IPEndPoint)chatty.Client.LocalEndPoint!);
            ulong silentAddress = MtaNetWrapper.BinaryAddressOf((IPEndPoint)silent.Client.LocalEndPoint!);

            // The open-connection request is the literal transcribed from capture.log seq 1. It is the
            // datagram that makes the wrapper allocate a session record, and it doubles as the
            // keep-alive below.
            byte[] openRequest = CaptureVectors.OpenRequestBytes;
            chatty.Send(openRequest, openRequest.Length, server);
            silent.Send(openRequest, openRequest.Length, server);

            Assert.True(
                WaitUntil(() => wrapper.SessionCount == 2, TimeSpan.FromSeconds(5)),
                "both peers should hold a session record after their open request");

            // The silent peer never sends again; its last datagram starts the timeout clock.
            (ulong Address, PacketId Id, byte[] Data, long ElapsedMilliseconds)? timeout = null;
            while (clock.Elapsed < TimeSpan.FromSeconds(20) && !timeout.HasValue)
            {
                chatty.Send(openRequest, openRequest.Length, server);
                Thread.Sleep(100);

                lock (gate)
                {
                    foreach (var notification in notifications)
                    {
                        if (notification.Id == PacketId.PACKET_ID_PLAYER_TIMEOUT &&
                            notification.Address == silentAddress)
                        {
                            timeout = notification;
                            break;
                        }
                    }
                }
            }

            Assert.True(timeout.HasValue, "the silent peer should be retired inside the poll window");
            Assert.True(
                timeout!.Value.ElapsedMilliseconds >= SessionTimeoutPolicy.TimeoutMilliseconds,
                $"retired after {timeout.Value.ElapsedMilliseconds} ms, before the " +
                $"{SessionTimeoutPolicy.TimeoutMilliseconds} ms timeout");
            Assert.True(
                timeout.Value.ElapsedMilliseconds <=
                    SessionTimeoutPolicy.TimeoutMilliseconds + (3 * SessionTimeoutPolicy.TickMilliseconds),
                $"retired after {timeout.Value.ElapsedMilliseconds} ms, more than three ticks past the " +
                $"{SessionTimeoutPolicy.TimeoutMilliseconds} ms timeout");

            lock (gate)
            {
                var delivered = Assert.Single(
                    notifications,
                    n => n.Id == PacketId.PACKET_ID_PLAYER_TIMEOUT && n.Address == silentAddress);
                Assert.Empty(delivered.Data);
                Assert.DoesNotContain(
                    notifications,
                    n => n.Id == PacketId.PACKET_ID_PLAYER_TIMEOUT && n.Address == chattyAddress);
            }

            Assert.Null(wrapper.FindSession(silentAddress));
            Assert.NotNull(wrapper.FindSession(chattyAddress));
            Assert.Equal(1, wrapper.SessionCount);
        }
        finally
        {
            wrapper.Stop();
        }
    }

    private static bool WaitUntil(Func<bool> condition, TimeSpan limit)
    {
        var clock = Stopwatch.StartNew();
        while (clock.Elapsed < limit)
        {
            if (condition())
                return true;

            Thread.Sleep(10);
        }

        return condition();
    }

    private static Socket BoundSocket(MtaNetWrapper wrapper)
    {
        const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
        FieldInfo field = typeof(MtaNetWrapper).GetField("socket", Flags)
            ?? throw new InvalidOperationException("MtaNetWrapper.socket is missing.");
        return (Socket?)field.GetValue(wrapper)
            ?? throw new InvalidOperationException("MtaNetWrapper is not started.");
    }

    private static ulong? LocalBinaryAddress(MtaNetWrapper wrapper)
    {
        const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
        FieldInfo field = typeof(MtaNetWrapper).GetField("localBinaryAddress", Flags)
            ?? throw new InvalidOperationException("MtaNetWrapper.localBinaryAddress is missing.");
        return (ulong?)field.GetValue(wrapper);
    }
}

/// <summary>
/// <see cref="FactAttribute"/> that skips unless <c>MTANET_SOCKET_TESTS=1</c>. xunit 2.x has no
/// dynamic skip, so the gate is applied when the test is discovered.
/// </summary>
internal sealed class SocketFactAttribute : FactAttribute
{
    internal const string GateVariable = "MTANET_SOCKET_TESTS";

    public SocketFactAttribute()
    {
        if (Environment.GetEnvironmentVariable(GateVariable) != "1")
            this.Skip = $"Socket test; set {GateVariable}=1 to run it.";
    }
}
