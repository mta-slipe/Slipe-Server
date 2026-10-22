using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SlipeServer.Net.Wrappers;
using SlipeServer.Net.Wrappers.Enums;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Net.Managed;

/// <summary>
/// A managed <see cref="INetWrapper"/> that speaks MTA:SA's network protocol directly, replacing the
/// native <c>net.dll</c>. It owns a UDP socket, speaks the modified RakNet framing and the modified
/// offline handshake, applies the inbound admission policy, and raises <see cref="PacketReceived"/>
/// with a 0-based MTA packet id and the stripped game payload, which is what
/// <c>MtaServer.EnqueueIncomingPacket</c> expects.
///
/// Lifecycle: bind in <see cref="Start"/>, close in <see cref="Stop"/>; a double start or double stop
/// throws <see cref="InvalidOperationException"/>.
/// </summary>
public sealed class MtaNetWrapper : INetWrapper, IDisposable
{
    /// <summary>The receive buffer size: 1492 bytes, the largest datagram the protocol carries.</summary>
    public const int MaxReceiveBytes = 1492;

    private readonly ConcurrentDictionary<ulong, MtaNetSession> sessions = new();
    private readonly byte[] serverGuid = new byte[16];
    private readonly object lifecycleLock = new();
    private readonly string host;
    private readonly ushort port;
    private readonly int playerCount;
    private readonly IMonotonicClock clock;
    private readonly Gate1RateLimiter gate1;
    private readonly IPeerVersionHints? peerVersionHints;

    private Socket? socket;
    private CancellationTokenSource? cancellation;
    private Task? receiveLoop;
    private Task? retransmitLoop;
    private Task? timeoutLoop;
    private volatile bool started;
    private bool disposed;

    /// <summary>
    /// This server's own endpoint, captured at bind time and advertised in the
    /// <c>RID_CONNECTION_REQUEST_ACCEPTED</c> address list. Null when the socket bound to
    /// <see cref="IPAddress.Any"/>, where there is no single address to advertise.
    /// </summary>
    private ulong? localBinaryAddress;

    /// <param name="clock">
    /// The monotonic clock every behavioural time read uses. Omit for the system clock; a test passes
    /// a manual clock so a timeout boundary can be reached without sleeping.
    /// </param>
    /// <param name="peerVersionHints">
    /// Where each session's client build is published so the ASE listener can advertise the matching
    /// version family. Omit to skip publishing.
    /// </param>
    public MtaNetWrapper(
        ushort port = 50666,
        string host = "0.0.0.0",
        int playerCount = 1024,
        string serverName = "C# server",
        IMonotonicClock? clock = null,
        IPeerVersionHints? peerVersionHints = null)
    {
        this.port = port;
        this.host = host;
        this.playerCount = playerCount;
        this.ServerName = serverName;
        this.peerVersionHints = peerVersionHints;

        this.clock = clock ?? SystemMonotonicClock.Instance;
        this.gate1 = new Gate1RateLimiter(this.clock);

        Random.Shared.NextBytes(this.serverGuid);

        this.JoinComplete.ServerVersionSortable = "1.6.0-9.0.0";
        this.JoinComplete.ConnectionText = "MTA: San Andreas [MtaNet POC]";
    }

    public event Action<INetWrapper, ulong, PacketId, byte[], uint?>? PacketReceived;

    /// <summary>Our 16-byte server GUID, echoed in every offline reply.</summary>
    public byte[] ServerGuid => this.serverGuid;

    public string ServerName { get; }

    public int PlayerCount => this.playerCount;

    public int SessionCount => this.sessions.Count;

    /// <summary>Inbound admission tunables. Every limit defaults to the protocol's "unlimited" encoding.</summary>
    public MtaAdmissionOptions AdmissionOptions { get; } = new();

    /// <summary>
    /// Which client build this server accepts: <see cref="MtaIdDialect.Release"/> for <c>net.dll</c>
    /// (the default), <see cref="MtaIdDialect.Debug"/> for <c>net_d.dll</c>, or
    /// <see cref="MtaIdDialect.Both"/> / <see cref="MtaIdDialect.Auto"/> to serve either build on one
    /// port. Forwarded to <see cref="AdmissionOptions"/>.
    /// </summary>
    public MtaIdDialect PeerDialect
    {
        get => this.AdmissionOptions.PeerDialect;
        set => this.AdmissionOptions.PeerDialect = value;
    }

    /// <summary>
    /// The inputs to the join-complete packet (packet 2). Each is a per-server-configuration value;
    /// see <see cref="ServerJoinCompleteOptions"/>.
    /// </summary>
    public ServerJoinCompleteOptions JoinComplete { get; } = new();

    /// <summary>
    /// When true, packet 2 is emitted lazily on the first admitted game packet from a session. Off by
    /// default: Slipe's <c>JoinDataPacketHandler</c> sends <c>JoinCompletePacket</c> itself once the
        /// password, ban and version checks pass, which is the correct order on the wire. Turning this on
    /// against a real game layer produces two packet 2s.
    /// </summary>
    public bool AutoSendJoinComplete { get; set; }

    /// <summary>Diagnostics sink. The console host wires this to its logger.</summary>
    public Action<string>? Trace { get; set; }

    /// <summary>
    /// When set, every datagram in both directions is appended to disk before any handler runs.
    /// Null (the default) means no capture and no per-datagram cost beyond a null check.
    /// </summary>
    public PacketCapture? Capture { get; set; }

    // ---------------------------------------------------------------- lifecycle

    public void Start()
    {
        lock (this.lifecycleLock)
        {
            if (this.started)
                throw new InvalidOperationException("Net server already started.");

            IPAddress address = this.host is "0.0.0.0" or "*" or ""
                ? IPAddress.Any
                : IPAddress.Parse(this.host);

            var bound = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            bound.Bind(new IPEndPoint(address, this.port));

            // Read the port back from the socket: port 0 means "any free port", and advertising the
            // configured value would tell every peer to reach us on port 0.
            var boundEndPoint = (IPEndPoint)bound.LocalEndPoint!;

            this.socket = bound;
            this.localBinaryAddress = address.Equals(IPAddress.Any)
                ? null
                : BinaryAddressOf(boundEndPoint);
            this.cancellation = new CancellationTokenSource();
            this.started = true;
            this.receiveLoop = Task.Run(() => this.ReceiveLoopAsync(bound, this.cancellation.Token));
            this.retransmitLoop = Task.Run(() => this.RetransmitLoopAsync(this.cancellation.Token));
            this.timeoutLoop = Task.Run(() => this.TimeoutLoopAsync(this.cancellation.Token));

            this.Trace?.Invoke($"listening on udp {boundEndPoint.Address}:{boundEndPoint.Port}");
        }
    }

    public void Stop()
    {
        lock (this.lifecycleLock)
        {
            if (!this.started)
                throw new InvalidOperationException("Net server is not running.");

            this.started = false;

            try
            {
                this.cancellation?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            this.socket?.Close();

            try
            {
                this.receiveLoop?.Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException)
            {
            }

            try
            {
                this.retransmitLoop?.Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException)
            {
            }

            try
            {
                this.timeoutLoop?.Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException)
            {
            }

            this.socket?.Dispose();
            this.socket = null;
            this.receiveLoop = null;
            this.retransmitLoop = null;
            this.timeoutLoop = null;
            this.cancellation?.Dispose();
            this.cancellation = null;

            // A stopped wrapper holds no session records and therefore retains no packets for a peer
            // that can no longer be reached.
            foreach (MtaNetSession session in this.sessions.Values)
                session.ReleaseReliabilityState();

            this.sessions.Clear();

            this.Trace?.Invoke("stopped");
        }
    }

    public void Dispose()
    {
        if (this.disposed)
            return;

        this.disposed = true;
        if (this.started)
            this.Stop();
    }

    // ---------------------------------------------------------------- receive

    private async Task ReceiveLoopAsync(Socket bound, CancellationToken token)
    {
        var buffer = new byte[MaxReceiveBytes];

        while (!token.IsCancellationRequested)
        {
            SocketReceiveFromResult result;
            try
            {
                EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                result = await bound
                    .ReceiveFromAsync(buffer, SocketFlags.None, remote, token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (SocketException exception)
            {
                // A UDP socket can report WSAECONNRESET when an ICMP port-unreachable comes back.
                // That is not fatal, but backing off stops a hot spin if it repeats.
                if (token.IsCancellationRequested)
                    return;

                this.Trace?.Invoke($"recv error: {exception.SocketErrorCode}");
                try
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                continue;
            }

            if (result.ReceivedBytes <= 0)
                continue;

            try
            {
                this.InjectDatagram(buffer, result.ReceivedBytes, (IPEndPoint)result.RemoteEndPoint);
            }
            catch (Exception exception)
            {
                this.Trace?.Invoke($"datagram handling threw: {exception}");
            }
        }
    }

    // ---------------------------------------------------------------- retransmission

    /// <summary>
    /// The timer that makes retransmission independent of traffic.
    ///
    /// <para>
    /// It is a dedicated per-wrapper loop rather than a piggyback on sends: a server with nothing to
    /// send still owes a retransmission, so a peer that has gone quiet (which is what a lossy link
    /// produces) would otherwise stall forever. A per-packet <c>Task.Delay</c> would not compose with
    /// an injected clock.
    /// </para>
    ///
    /// <para>
    /// Every deadline is compared against <see cref="IMonotonicClock"/>. The single <c>Task.Delay</c>
    /// is the loop's pacing, one delay per tick; tests bypass it by calling
    /// <see cref="TickRetransmissions"/> directly.
    /// </para>
    /// </summary>
    private async Task RetransmitLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(RetransmissionPolicy.TickMilliseconds, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                this.TickRetransmissions();
            }
            catch (Exception exception)
            {
                this.Trace?.Invoke($"retransmission tick threw: {exception}");
            }
        }
    }

    /// <summary>
    /// One retransmission scan over every live session: each reliable packet whose timeout has
    /// expired is rebuilt from its stored <see cref="OutgoingPacket"/> and resent.
    ///
    /// <para>
    /// The rebuild reuses the original sequence number, ordering index and split header, so the peer
    /// dedupes on sequence number and orders on ordering index exactly as it did the first time.
    /// Acks are passed as empty: the first send consumed them, and re-attaching would duplicate them.
    /// The resend goes through <see cref="SendRaw"/>, so it appears in <see cref="PacketCapture"/> with
    /// the same <c>seq</c> as the original.
    /// </para>
    ///
    /// <para>
    /// Internal so a test can tick deterministically. Returns the number of datagrams resent.
    /// </para>
    /// </summary>
    internal int TickRetransmissions()
    {
        int resent = 0;
        ulong now = this.NowMilliseconds;

        foreach (MtaNetSession session in this.sessions.Values)
        {
            var resends = new List<PendingReliablePacket>();
            var giveUps = new List<PendingReliablePacket>();

            lock (session.ReliabilitySyncRoot)
            {
                if (session.UnackedReliable.Count == 0)
                    continue;

                foreach (PendingReliablePacket pending in session.UnackedReliable.Values)
                {
                    if (now - pending.LastSentAtMilliseconds < RetransmissionPolicy.TimeoutMilliseconds)
                        continue;

                    if (pending.RetransmissionCount >= RetransmissionPolicy.MaximumRetransmissions)
                    {
                        giveUps.Add(pending);
                        continue;
                    }

                    // Recorded under the lock, so a concurrent ack cannot clear an entry that is
                    // already scheduled for a resend and leave the attempt counter climbing.
                    pending.RecordRetransmission(now);
                    resends.Add(pending);
                }

                foreach (PendingReliablePacket pending in giveUps)
                    session.UnackedReliable.Remove(pending.Packet.SequenceNumber);
            }

            foreach (PendingReliablePacket pending in resends)
            {
                byte[] datagram = RakNetDatagramWriter.Build(
                    now, Array.Empty<AckRange>(), new[] { pending.Packet });
                this.SendRaw(datagram, session.EndPoint);
                resent++;

                this.Trace?.Invoke(
                    $"retransmit seq={pending.Packet.SequenceNumber} attempt " +
                    $"{pending.RetransmissionCount}/{RetransmissionPolicy.MaximumRetransmissions} to {session.EndPoint}");
            }

            foreach (PendingReliablePacket pending in giveUps)
                this.GiveUpOnPacket(session, pending, now);
        }

        return resent;
    }

    /// <summary>
    /// Abandons a reliable packet after its last allowed resend; the ledger entry is already gone by
    /// the time this runs.
    ///
    /// <para>
    /// The session is retired: a peer that has not acknowledged six transmissions of the same packet
    /// is unreachable, and keeping the record would retain every other unacked packet for it and let
    /// the timer keep resending into a black hole. What the native stack does after its own
    /// retransmission limit is not established; "give up and retire" is this port's choice, not a
    /// confirmed match.
    /// </para>
    /// </summary>
    private void GiveUpOnPacket(MtaNetSession session, PendingReliablePacket pending, ulong now)
    {
        this.Trace?.Invoke(
            $"retransmit give-up: seq={pending.Packet.SequenceNumber} to {session.EndPoint} " +
            $"unacknowledged after {pending.RetransmissionCount} retransmission(s) over " +
            $"{now - pending.FirstSentAtMilliseconds} ms; retiring the session");

        // A peer that has ignored six transmissions is as gone as one that has stopped sending, so it
        // is retired through the same path: the game layer hears PACKET_ID_PLAYER_TIMEOUT before the
        // record is removed.
        this.RetireWithLifecyclePacket(
            session,
            SynthesiseSystemRecord(MtaProtocol.Rid.ConnectionLost),
            MtaGamePackets.PlayerTimeoutMtaId);
    }

    // ---------------------------------------------------------------- liveness

    /// <summary>
    /// Paces <see cref="TickTimeouts"/> as <see cref="RetransmitLoopAsync"/> paces
    /// <see cref="TickRetransmissions"/>: one <c>Task.Delay</c> per tick, never one per session, and
    /// every deadline compared against <see cref="IMonotonicClock"/>. Tests bypass the delay by
    /// calling the tick directly and advancing a manual clock.
    /// </summary>
    private async Task TimeoutLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(SessionTimeoutPolicy.TickMilliseconds, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                this.TickTimeouts();
            }
            catch (Exception exception)
            {
                this.Trace?.Invoke($"timeout tick threw: {exception}");
            }
        }
    }

    /// <summary>
    /// One silence scan over every live session: a peer whose last accepted datagram is at least
    /// <see cref="SessionTimeoutPolicy.TimeoutMilliseconds"/> old is retired.
    ///
    /// <para>
    /// A dead peer sends no <c>RID_CONNECTION_LOST</c>; RakNet fabricates that event locally once the
    /// peer's last datagram ages past the timeout. This sweep stands in for that, and the fabricated
    /// event takes the same path a wire one would, ending in mtaId 6 (player timeout).
    /// </para>
    ///
    /// <para>
    /// Internal so a test can tick deterministically. Returns the number of peers retired.
    /// </para>
    /// </summary>
    internal int TickTimeouts()
    {
        int retired = 0;
        ulong now = this.NowMilliseconds;

        // Snapshot so the sweep judges only the peers that existed when it started; a peer that
        // joins mid-scan is judged by the next tick rather than by a `now` captured before it existed.
        foreach (MtaNetSession session in this.sessions.Values.ToArray())
        {
            ulong silentFor = now - session.LastReceivedAtMilliseconds;
            if (silentFor < SessionTimeoutPolicy.TimeoutMilliseconds)
                continue;

            this.Trace?.Invoke(
                $"session for {session.EndPoint} silent for {silentFor} ms " +
                $"(timeout {SessionTimeoutPolicy.TimeoutMilliseconds} ms); synthesising RID_CONNECTION_LOST");

            if (this.RetireWithLifecyclePacket(
                session,
                SynthesiseSystemRecord(MtaProtocol.Rid.ConnectionLost),
                MtaGamePackets.PlayerTimeoutMtaId))
            {
                retired++;
            }
        }

        return retired;
    }

    /// <summary>
    /// A one-byte RakNet system record carrying <paramref name="rid"/>, shaped exactly like a parsed
    ///     wire record. Used for events RakNet produces locally instead of reading off the wire; a
        /// fabricated <c>RID_CONNECTION_LOST</c> is indistinguishable from a real one, and
        /// <see cref="DeliverSystemDerived"/> strips the id byte the same way either way.
    /// </summary>
    private static RakNetPacketRecord SynthesiseSystemRecord(byte rid) =>
        new()
        {
            BitCount = 8,
            Payload = new[] { rid },
        };

    /// <summary>
    /// Clears the retransmission ledger for every sequence number covered by <paramref name="acks"/>.
    /// <see cref="AckRange"/> is inclusive at both ends.
    ///
    /// <para>
    /// The ledger is walked rather than the range: a peer can claim a range of four billion sequence
    /// numbers, and iterating that would be a denial of service. Unknown sequence numbers are simply
    /// not found, which is not an error.
    /// </para>
    /// </summary>
    private int Acknowledge(MtaNetSession session, IReadOnlyList<AckRange> acks)
    {
        int cleared = 0;

        lock (session.ReliabilitySyncRoot)
        {
            if (session.UnackedReliable.Count == 0)
                return 0;

            var covered = new List<uint>();

            foreach (AckRange range in acks)
            {
                if (range.End < range.Start)
                    continue;

                covered.Clear();

                foreach (uint sequence in session.UnackedReliable.Keys)
                {
                    if (sequence >= range.Start && sequence <= range.End)
                        covered.Add(sequence);
                }

                foreach (uint sequence in covered)
                {
                    session.UnackedReliable.Remove(sequence);
                    cleared++;
                }
            }
        }

        return cleared;
    }

    /// <summary>
    /// The single entry point for one received datagram. Both the UDP socket and the loopback test
    /// harness call this, so a test exercises exactly the path the wire feeds.
    /// </summary>
    public void InjectDatagram(byte[] datagram, int byteLength, IPEndPoint remote)
    {
        if (byteLength < 3)
        {
            this.Capture?.Record(
                CaptureDirection.Rx, remote, datagram, byteLength, CaptureDescribe.Route(datagram, byteLength, false));
            return;
        }

        ulong binaryAddress = BinaryAddressOf(remote);

        // Liveness is stamped before any gate. A peer whose packets are all dropped by admission, a
        // rate limiter or the 8-byte floor is still sending, so "alive" means a datagram arrived,
        // not that a packet was delivered.
        bool hasSessionRecord = this.sessions.TryGetValue(binaryAddress, out MtaNetSession? liveSession);
        liveSession?.MarkReceived(this.NowMilliseconds);

        // Capture before routing, so a datagram that fails to parse or whose handler throws still
        // leaves its bytes on disk with the verdict the router reached.
        this.Capture?.Record(
            CaptureDirection.Rx,
            remote,
            datagram,
            byteLength,
            CaptureDescribe.Route(datagram, byteLength, hasSessionRecord));

        switch (OfflineHandshake.Route(datagram, byteLength, hasSessionRecord))
        {
            case OfflineRoute.Offline:
                this.HandleOffline(datagram, byteLength, remote, binaryAddress);
                break;

            case OfflineRoute.Connected:
                this.HandleConnected(datagram, byteLength, binaryAddress);
                break;

            default:
                // No session and no valid magic: dropped.
                break;
        }
    }

    /// <summary>
    /// The 64-bit peer identifier: <c>(ipDword &lt;&lt; 32) + port</c>, where <c>ipDword</c> is the
    /// IPv4 address bytes read as a little-endian dword. Non-IPv4 addresses map to 0.
    /// </summary>
    public static ulong BinaryAddressOf(IPEndPoint remote)
    {
        byte[] bytes = remote.Address.GetAddressBytes();
        if (bytes.Length != 4)
            return 0;

        uint ipDword = (uint)bytes[0]
            | ((uint)bytes[1] << 8)
            | ((uint)bytes[2] << 16)
            | ((uint)bytes[3] << 24);

        return ((ulong)ipDword << 32) + (uint)remote.Port;
    }

    // ---------------------------------------------------------------- offline handshake

    private void HandleOffline(byte[] datagram, int byteLength, IPEndPoint remote, ulong binaryAddress)
    {
        switch (datagram[0])
        {
            case OfflineHandshake.CasePing:
                if (OfflineHandshake.TryReadPing(datagram, byteLength, out ulong clientTime))
                {
                    this.SendRaw(OfflineHandshake.BuildPong(clientTime, this.serverGuid), remote);
                    this.Trace?.Invoke($"ping from {remote} -> pong (41 B)");
                }

                break;

            case OfflineHandshake.CaseOpenConnectionRequest:
                this.HandleOpenConnectionRequest(datagram, byteLength, remote, binaryAddress);
                break;

            case OfflineHandshake.CaseMtaHandshake38:
                if (OfflineHandshake.TryReadMtaHandshake38(datagram, byteLength, out byte subtype, out uint echo))
                {
                    // Subtypes 0 and 2 get no reply; only subtype 1 is answered.
                    if (subtype == 1)
                    {
                        // The reply carries a 32-bit tick count: the low 32 bits of the injected
                        // monotonic clock, which keeps the reply byte-deterministic under a manual clock.
                        this.SendRaw(
                            OfflineHandshake.BuildMtaHandshake38Reply(
                                this.serverGuid, echo, (uint)this.NowMilliseconds),
                            remote);
                        this.Trace?.Invoke($"0x26 subtype 1 from {remote} -> reply (26 B)");
                    }
                    else
                    {
                        this.Trace?.Invoke($"0x26 subtype {subtype} from {remote} -> no reply");
                    }
                }

                break;

            default:
                // RID_CONNECTION_REQUEST (4) passes magic validation and is intentionally dropped
                // without a reply: the server has no handler for it.
                this.Trace?.Invoke($"offline id 0x{datagram[0]:X2} from {remote} -> dropped");
                break;
        }
    }

    private void HandleOpenConnectionRequest(
        byte[] datagram, int byteLength, IPEndPoint remote, ulong binaryAddress)
    {
        if (!OfflineHandshake.TryReadOpenConnectionRequest(datagram, byteLength, out byte[] clientGuid, out byte versionByte))
            return;

        // A version byte other than 0x04 is refused: [0x5F][0x04][magic 16][our GUID 16].
        if (versionByte != OfflineHandshake.OpenConnectionRequestVersion)
        {
            this.SendRaw(OfflineHandshake.BuildRefusal(MtaProtocol.Rid.Refusal95, this.serverGuid), remote);
            this.Trace?.Invoke($"open-connection version 0x{versionByte:X2} from {remote} -> 0x5F refusal (34 B)");
            return;
        }

        if (this.sessions.Count >= this.playerCount)
        {
            this.SendRaw(
                OfflineHandshake.BuildRefusal(MtaProtocol.Rid.NoFreeIncomingConnections, this.serverGuid),
                remote);
            this.Trace?.Invoke($"no free slots for {remote} -> 0x12 refusal (33 B)");
            return;
        }

        // Look up before creating, so a repeated open-connection request from a connected peer does
        // not allocate a throwaway session: GetOrAdd runs its factory even when the key exists, and a
        // session is ~30 KiB of tables.
        if (!this.sessions.TryGetValue(binaryAddress, out MtaNetSession? session))
        {
            var created = new MtaNetSession(binaryAddress, remote);

            // The record exists because this datagram arrived, so it starts alive.
            created.MarkReceived(this.NowMilliseconds);
            session = this.sessions.GetOrAdd(binaryAddress, created);
        }

        // A peer that already has a completed handshake gets no reply.
        if (session.HandshakeComplete)
        {
            this.Trace?.Invoke($"open-connection from already-connected {remote} -> no reply");
            return;
        }

        Array.Copy(clientGuid, session.Guid, 16);
        session.HandshakeComplete = true;
        session.IsEstablished = true;

        this.SendRaw(OfflineHandshake.BuildOpenConnectionReply(this.serverGuid), remote);
        this.Trace?.Invoke($"open-connection accepted for {remote} -> reply (33 B)");
    }

    // ---------------------------------------------------------------- connected path

    private void HandleConnected(byte[] datagram, int byteLength, ulong binaryAddress)
    {
        if (!this.sessions.TryGetValue(binaryAddress, out MtaNetSession? session))
            return;

        // A connected datagram shorter than 8 bytes is rejected before any packet inside it is read.
        if (byteLength < MtaAdmission.MinimumDatagramBytes)
        {
            this.Trace?.Invoke($"datagram of {byteLength} B from {session.EndPoint} is below the 8-byte game-path floor");
            return;
        }

        if (!RakNetDatagram.TryParse(datagram, byteLength, out RakNetDatagram parsed, out string error))
        {
            this.Trace?.Invoke($"datagram parse failed from {session.EndPoint}: {error}");
            return;
        }

        if (parsed.HasAcks)
        {
            // The peer is acknowledging our outbound reliable packets, which clears the
            // retransmission ledger. Each range is inclusive at both ends. An ack for a sequence
            // number never sent is not an error, and neither is a duplicate ack arriving after a
            // resend.
            int cleared = this.Acknowledge(session, parsed.Acks);
            this.Trace?.Invoke(
                $"peer acked {parsed.Acks.Count} range(s), cleared {cleared} outbound packet(s), " +
                $"{session.UnackedReliableCount} still in flight");
        }

        foreach (RakNetPacketRecord record in parsed.Packets)
        {
            this.HandleGamePacket(session, record);
        }

        // Anything still pending earned no piggyback slot (a send happens only when a handler
        // replies), so it goes out on its own; otherwise the peer retransmits forever.
        this.FlushAcks(session);
    }

    /// <summary>
    /// The reliability layer's inbound stages, in order: ack, dedupe, split reassembly, then
    /// ordering. Everything below the ordering stage is <see cref="DeliverGamePacket"/>.
    /// </summary>
    private void HandleGamePacket(MtaNetSession session, RakNetPacketRecord record)
    {
        if (record.Payload.Length == 0)
            return;

        // The ack is queued before any admission decision: acking belongs to the reliability layer,
        // upstream of the admission gates.
        lock (session.ReliabilitySyncRoot)
            session.PendingAcks.Add(new AckRange(record.SequenceNumber, record.SequenceNumber));

        // A retransmission repeats its sequence number. It is acked again but not re-delivered, so a
        // peer still waiting for its reply is not answered once per retry.
        if (!session.TrackReceivedSequence(record.SequenceNumber))
        {
            this.Trace?.Invoke(
                $"duplicate seq {record.SequenceNumber} from {session.EndPoint} -> re-acked, not re-delivered");
            return;
        }

        // A split packet is one fragment of a larger logical message. Reassembly happens in the
        // reliability layer because a part's first byte is a mid-message byte, not a packet id.
        int reassembledParts = 0;
        if (record.HasSplitData)
        {
            switch (session.InboundSplits.Accept(record, this.NowMilliseconds, out RakNetPacketRecord? whole))
            {
                case SplitAcceptance.Complete:
                    reassembledParts = (int)record.SplitPacketCount;
                    this.Trace?.Invoke(
                        $"reassembled sid={record.SplitPacketId} from {reassembledParts} part(s): {whole!.BitCount} bits ({whole.Payload.Length} B)");
                    record = whole!;
                    break;

                case SplitAcceptance.Buffered:
                    this.Trace?.Invoke(
                        $"buffered split part {record.SplitPacketIndex}/{record.SplitPacketCount} sid={record.SplitPacketId} from {session.EndPoint}");
                    return;

                default:
                    this.Trace?.Invoke(
                        $"discarded split part {record.SplitPacketIndex}/{record.SplitPacketCount} sid={record.SplitPacketId} from {session.EndPoint}");
                    return;
            }
        }

        // Ordering is the last reliability-layer stage: records are delivered in ordering-index
        // order, not arrival order, before the game layer sees anything. A record held here is
        // already acknowledged above, so ordering never delays the ack path.
        switch (session.InboundOrdering.Accept(record, this.NowMilliseconds, out RakNetPacketRecord[] ready))
        {
            case OrderingAcceptance.Buffered:
                this.Trace?.Invoke(
                    $"held ordered ch={record.OrderingChannel} oi={record.OrderingIndex} from {session.EndPoint} behind the hole");
                return;

            case OrderingAcceptance.Discarded:
                this.Trace?.Invoke(
                    $"discarded ch={record.OrderingChannel} oi={record.OrderingIndex} (rel {record.Reliability}) from {session.EndPoint}: older than expected, repeated, or over a bound");
                return;
        }

        // A hole that fills can release the record that filled it plus every record held behind it.
        foreach (RakNetPacketRecord deliverable in ready)
        {
            this.DeliverGamePacket(
                session,
                deliverable,
                ReferenceEquals(deliverable, record) ? reassembledParts : 0);
        }
    }

    /// <summary>
    /// Admission and delivery for one record that the reliability layer released, in ordering order.
    /// </summary>
    private void DeliverGamePacket(
        MtaNetSession session, RakNetPacketRecord record, int reassembledParts)
    {
        byte[] payload = record.Payload;

        // Wire ids below RID_USER_PACKET_ENUM are RakNet system messages, not MTA game packets; they
        // go to the system-message dispatcher and never reach the MTA admission gates.
        if (payload[0] < MtaProtocol.MtaIdToWireId)
        {
            this.HandleRakNetSystemPacket(session, record);
            return;
        }

        MtaAdmissionResult result = MtaAdmission.Evaluate(
            session,
            payload[0],
            payload,
            record.BitCount,
            this.AdmissionOptions,
            out byte mtaId,
            out string reason,
            this.gate1);

        if (result != MtaAdmissionResult.Deliver)
        {
            this.Trace?.Invoke($"dropped wire id 0x{payload[0]:X2} (mta {mtaId}): {reason}");
            return;
        }

        // The game payload is the record minus its leading id byte: bitLen - 1 bits starting at byte 1.
        int strippedBits = record.BitCount - 1;
        if (strippedBits < 0)
            strippedBits = 0;

        int strippedBytes = (strippedBits + 7) >> 3;
        if (strippedBytes > payload.Length - 1)
            strippedBytes = payload.Length - 1;

        var data = new byte[strippedBytes];
        Array.Copy(payload, 1, data, 0, strippedBytes);

        if (mtaId == MtaGamePackets.PlayerJoinDataMtaId)
        {
            // The serial is a net-layer field, XOR-obfuscated ahead of the mod-level body, so it has
            // to be read before ExtractPlayerJoinDataBlock drops the prefix.
            bool haveSerial = MtaGamePackets.TryReadPlayerJoinDataSerial(data, out byte[] serialBytes);

            // The client writes only the join body, and the net layer's opaque prefix precedes it on
            // the wire. Strip the prefix before the mod-level reader.
            byte[] joinBody = MtaGamePackets.ExtractPlayerJoinDataBlock(data);
            if (!ReferenceEquals(joinBody, data))
            {
                this.Trace?.Invoke(
                    $"join-data body at payload offset {data.Length - joinBody.Length} ({joinBody.Length} B, {data.Length - joinBody.Length} B prefix dropped)");
                data = joinBody;
            }
            else
            {
                this.Trace?.Invoke($"join-data body not located; passing {data.Length} B through unchanged");
            }

            // Slipe reads these back synchronously from JoinDataPacketHandler via
            // IClient.FetchSerial() -> INetWrapper.GetClientSerialExtraAndVersion, and the
            // PLAYER_INGAME_NOTICE RPC later needs the version to reach SERVER_JOINEDGAME.
            bool haveVersion = MtaGamePackets.TryReadPlayerJoinDataVersion(data, out string playerVersion);
            if (haveVersion)
            {
                session.ClientVersion = playerVersion;

                // The ASE listener needs to know which version family to advertise to this peer,
                // because a client aborts unless the ver string it reads matches its own build.
                if (this.peerVersionHints != null
                    && MtaPeerBuildTypeParser.TryParsePlayerVersion(playerVersion, out MtaPeerBuildType build))
                {
                    this.peerVersionHints.Record(session.EndPoint.Address, build);
                }
            }

            if (haveSerial)
                session.Serial = Encoding.ASCII.GetString(serialBytes);

            if (haveVersion || haveSerial)
            {
                this.Trace?.Invoke(
                    $"client identity: version=\"{(haveVersion ? session.ClientVersion : "(unreadable)")}\" serial={(haveSerial ? session.Serial : "(unreadable)")}");
            }
            else
            {
                this.Trace?.Invoke("client identity not readable from the join packet; leaving it unset");
            }
        }

        // The suffix is the only live evidence that a client fragmented a message: otherwise a
        // reassembled split looks like an ordinary large packet.
        string split = reassembledParts > 0 ? $" reassembled={reassembledParts} parts" : string.Empty;

        this.Trace?.Invoke(
            $"DELIVER mtaId={mtaId} (wire 0x{payload[0]:X2}) {data.Length} B from {session.EndPoint}{split}");

        // The ping argument is nullable and is only non-null for a ping-derived packet; offline ping
        // replies never reach the reducer, so game packets always carry null here.
        this.PacketReceived?.Invoke(this, session.BinaryAddress, (PacketId)mtaId, data, null);

        if (this.AutoSendJoinComplete)
            this.TrySendJoinComplete(session);
    }

    /// <summary>
    /// The connected RakNet system-message dispatcher. Id 4 has its own handler; 0x11, 0x13 and 0x14
    /// are turned into MTA game packets (join, quit, timeout).
    /// </summary>
    private void HandleRakNetSystemPacket(MtaNetSession session, RakNetPacketRecord record)
    {
        byte[] payload = record.Payload;

        switch (payload[0])
        {
            case MtaProtocol.Rid.ConnectionRequest:
                this.HandleConnectionRequest(session, payload);
                break;

            case MtaProtocol.Rid.NewIncomingConnection:
                // Raised as MTA id 3 (PACKET_ID_PLAYER_JOIN) with the RakNet body stripped. Stock
                // RakNet's 0x11 body is empty, so the game layer receives a zero-length payload;
                // Slipe's JoinedGamePacketHandler tolerates that (Read is a no-op).
                this.DeliverSystemDerived(session, record, MtaGamePackets.PlayerJoinMtaId);
                break;

            case MtaProtocol.Rid.DisconnectionNotification:
                // Raised as MTA id 5 (PACKET_ID_PLAYER_QUIT). The client sends this when it gives up,
                // so a session that reaches the join stages normally never sends it; delivering it
                // lets Slipe retire the IClient if a client does bail.
                this.RetireWithLifecyclePacket(session, record, MtaGamePackets.PlayerQuitMtaId);
                break;

            case MtaProtocol.Rid.ConnectionLost:
                // Raised as MTA id 6 (PACKET_ID_PLAYER_TIMEOUT), the same shape as 0x13.
                //
                // A live peer does not send this: RakNet fabricates RID_CONNECTION_LOST locally when
                // a peer's last datagram ages past the timeout, so the common producer is
                // TickTimeouts, not this arm. A wire-borne one is handled identically.
                this.RetireWithLifecyclePacket(session, record, MtaGamePackets.PlayerTimeoutMtaId);
                break;

            default:
                this.Trace?.Invoke(
                    $"RakNet id 0x{payload[0]:X2} ({MtaProtocol.RidName(payload[0])}) from {session.EndPoint} -> no handler");
                break;
        }
    }

    /// <summary>
    /// Drops a session record so its binary address can open a new connection. Removal is by
    /// identity, not by key, so a concurrent reconnect's new session is never removed by mistake.
    /// </summary>
    private void RemoveSession(MtaNetSession session)
    {
        if (!this.sessions.TryRemove(new KeyValuePair<ulong, MtaNetSession>(session.BinaryAddress, session)))
            return;

        // Everything retained for this peer goes with it: nothing will ever ack its ledger entries,
        // and the timer would keep resending to a dead endpoint.
        session.ReleaseReliabilityState();

        this.Trace?.Invoke(
            $"session for {session.EndPoint} retired ({this.sessions.Count} of {this.playerCount} slot(s) in use)");
    }

    /// <summary>
    /// The one path that turns a RakNet connection event into a terminal MTA packet and then drops the
    /// record. Every retirement caused by the peer going away goes through here (inbound 0x13,
    /// inbound or fabricated 0x14, and the retransmission give-up), so the game layer is told exactly
    /// once per peer.
    ///
    /// <para>
    /// Delivery happens <em>before</em> <see cref="RemoveSession"/>: Slipe needs the notification
    /// while the binary address is still meaningful, but the address must be free afterwards. A
    /// session that outlives its peer makes <see cref="HandleOpenConnectionRequest"/>'s
    /// "already-connected" arm answer the peer's next attempt with silence, so a client reconnecting
    /// from the same source port could never get back in.
    /// </para>
    ///
    /// <para>
    /// The one-shot claim in <see cref="MtaNetSession.TryClaimRetirement"/> guarantees exactly-once:
    /// the sweep and an inbound teardown can both reach the same peer, and whichever gets there first
    /// notifies. Returns true when this call was the one that retired the peer.
    /// </para>
    /// </summary>
    private bool RetireWithLifecyclePacket(MtaNetSession session, RakNetPacketRecord record, byte mtaId)
    {
        if (!session.TryClaimRetirement())
            return false;

        this.DeliverSystemDerived(session, record, mtaId);
        this.RemoveSession(session);
        return true;
    }

    /// <summary>
    /// Delivers a game packet synthesised from a RakNet system message rather than from an MTA-band
    /// wire id. The system id byte is stripped the same way an MTA wire id is.
    /// </summary>
    private void DeliverSystemDerived(MtaNetSession session, RakNetPacketRecord record, byte mtaId)
    {
        int strippedBits = record.BitCount - 1;
        if (strippedBits < 0)
            strippedBits = 0;

        int strippedBytes = (strippedBits + 7) >> 3;
        if (strippedBytes > record.Payload.Length - 1)
            strippedBytes = record.Payload.Length - 1;

        var data = new byte[strippedBytes];
        Array.Copy(record.Payload, 1, data, 0, strippedBytes);

        this.Trace?.Invoke(
            $"DELIVER mtaId={mtaId} (from RakNet 0x{record.Payload[0]:X2}) {strippedBytes} B from {session.EndPoint}");

        this.PacketReceived?.Invoke(this, session.BinaryAddress, (PacketId)mtaId, data, null);
    }

    /// <summary>
    /// The connected <c>RID_CONNECTION_REQUEST</c> handler. A server with a configured password
    /// would answer a bad request with <c>RID_INVALID_PASSWORD</c> (0x17); this one has no password,
    /// so a well-formed request is always accepted.
    /// </summary>
    private void HandleConnectionRequest(MtaNetSession session, byte[] payload)
    {
        if (!ConnectedHandshake.TryReadConnectionRequest(payload, out byte[] clientGuid))
        {
            this.Trace?.Invoke(
                $"malformed RID_CONNECTION_REQUEST ({payload.Length} B) from {session.EndPoint} -> dropped");
            return;
        }

        Array.Copy(clientGuid, session.Guid, 16);
        session.ConnectionAccepted = true;

        ulong[] serverAddresses = this.localBinaryAddress is ulong local
            ? new[] { local }
            : Array.Empty<ulong>();

        byte[] body = ConnectedHandshake.BuildConnectionRequestAccepted(
            session.BinaryAddress, serverAddresses, ConnectedHandshake.AssumedSystemIndex);

        this.Trace?.Invoke(
            $"RID_CONNECTION_REQUEST from {session.EndPoint} -> 0x0E accepted ({body.Length} B)");

        // Sent RELIABLE on ordering channel 0.
        this.SendWirePayload(session, body, WireReliability.Reliable);
    }

    /// <summary>
    /// Emits a datagram that carries only the queued acks. Acks are sent as soon as a datagram is
    /// consumed; at this point in the handshake there is no other outbound traffic to carry them.
    /// </summary>
    private void FlushAcks(MtaNetSession session)
    {
        AckRange[] acks;

        // PendingAcks is appended from the receive path and drained from the send path and the
        // post-datagram flush, so the read-and-clear is atomic.
        lock (session.ReliabilitySyncRoot)
        {
            if (session.PendingAcks.Count == 0)
                return;

            acks = session.PendingAcks.ToArray();
            session.PendingAcks.Clear();
        }

        byte[] datagram = RakNetDatagramWriter.Build(this.NowMilliseconds, acks, Array.Empty<OutgoingPacket>());
        this.SendRaw(datagram, session.EndPoint);
    }

    private void TrySendJoinComplete(MtaNetSession session)
    {
        if (session.JoinCompleteSent)
            return;

        session.JoinCompleteSent = true;

        // The mod-level body only: SendPacket prepends the version-gated leading block, so this must
        // not pre-frame it.
        var bs = new BitStream(160);
        bs.WriteString(this.JoinComplete.ConnectionText ?? string.Empty);
        bs.WriteString(this.JoinComplete.ServerVersionSortable ?? string.Empty);
        byte[] body = bs.ToArray();

        ushort version = session.NegotiatedBitStreamVersion != 0
            ? session.NegotiatedBitStreamVersion
            : this.JoinComplete.BitStreamVersion;

        this.Trace?.Invoke($"sending packet 2 (SERVER_JOIN_COMPLETE) {body.Length} B body to {session.EndPoint}");
        this.SendPacket(
            session.BinaryAddress,
            PacketId.PACKET_ID_SERVER_JOIN_COMPLETE,
            version,
            body);
    }

    // ---------------------------------------------------------------- send

    /// <summary>
    /// The registry player count that packets such as PLAYER_LIST and PLAYER_QUIT carry on the wire:
    /// the number of connected peers, clamped to 16 bits.
    /// </summary>
    private ushort RegistryPlayerCount => (ushort)Math.Min(this.sessions.Count, ushort.MaxValue);

    public void SendPacket(ulong binaryAddress, ushort bitStreamVersion, Packet packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        this.SendPacket(
            binaryAddress,
            packet.PacketId,
            bitStreamVersion,
            packet.Write(),
            packet.Priority,
            packet.Reliability);
    }

    public void SendPacket(
        ulong binaryAddress,
        PacketId packetId,
        ushort bitStreamVersion,
        byte[] data,
        PacketPriority priority = PacketPriority.High,
        PacketReliability reliability = PacketReliability.ReliableSequenced)
    {
        if (!this.sessions.TryGetValue(binaryAddress, out MtaNetSession? session))
        {
            this.Trace?.Invoke($"SendPacket {packetId} for unknown peer 0x{binaryAddress:X16} -> dropped");
            return;
        }

        // wire_id = 99 + mta_id (RID_USER_PACKET_ENUM), with no remap: egress is a bare `add al,63h`
        // in both net.dll and net_d.dll. Packet 2 therefore travels as 0x65 and PLAYER_JOINDATA as
        // 0x67; the client's own ingress decides whether that means 4 or 0x13 on its side.
        byte wireId = MtaProtocol.ToWireId((byte)(int)packetId);

        // Packet 2 is the one packet whose wire body is not the mod-level packet body: a
        // version-gated leading block precedes the CPlayerConnectCompletePacket bytes. Slipe's
        // JoinCompletePacket supplies only those bytes, so the block is prepended here. The gate is
        // the client's own negotiated bitstream version.
        if (packetId == (PacketId)MtaGamePackets.ServerJoinCompleteMtaId &&
            MtaGamePackets.TryReadJoinCompleteBody(data, out string connectionText, out string serverVersion))
        {
            ushort negotiated = session.NegotiatedBitStreamVersion != 0
                ? session.NegotiatedBitStreamVersion
                : bitStreamVersion;

            var options = new ServerJoinCompleteOptions
            {
                BitStreamVersion = negotiated != 0 ? negotiated : this.JoinComplete.BitStreamVersion,
                ConnectionText = connectionText,
                ServerVersionSortable = serverVersion,
                // The serial travels as ASCII: 32 hex characters. ASCII is deliberate so a stray
                // non-ASCII byte cannot widen it.
                ClientSerial = Encoding.ASCII.GetBytes(session.Serial ?? string.Empty),
                AcDisableBitmapLow = this.JoinComplete.AcDisableBitmapLow,
                AcDisableBitmapHigh = this.JoinComplete.AcDisableBitmapHigh,
                ImageModFlags = this.JoinComplete.ImageModFlags,
                EnableClientChecks = this.JoinComplete.EnableClientChecks,
                AdapterBlock = this.JoinComplete.AdapterBlock,
            };

            byte[] framed = MtaGamePackets.BuildServerJoinComplete(options);
            this.Trace?.Invoke(
                $"packet 2 framed with the leading block: {data.Length} B body -> {framed.Length} B " +
                $"at bitstream version 0x{options.BitStreamVersion:X}");

            data = framed;
        }

        // PLAYER_LIST: the net layer writes an anticheat block ahead of the game payload, and the
        // client consumes the leading u16 of it before handing the stream to its packet handler.
        //
        // That leaves the two length-prefixed anticheat strings as the first thing the game layer
        // reads. The `bJustJoined` bit that follows is not a separate field: the client's writer
        // emits `WriteBit(m_bShowInChat)` there, so Slipe's leading `showInChat` bit already supplies
        // it and the entries follow immediately.
        //
        // An extra empty-string envelope on top of the anticheat block would misalign the body by 32
        // bits: bJustJoined would read 0 and the entry loop would start 32 bits early, so the first
        // nick length read is 0, which is protocol error 9.
        //
        // The rewrite lives in the transport, next to the packet-2 block, rather than in the game
        // layer. Wire order: 0x7C, (anticheat block), showInChat/bJustJoined, entries.
        if (packetId == (PacketId)MtaGamePackets.PlayerListMtaId)
        {
            // The roster's anticheat-block gate is evaluated against the client's negotiated
            // bitstream version (0x91 for a 1.6.0 client): the session's value when known, otherwise
            // the SendPacket parameter. Both gate terms use that same value. The long form (registry
            // u16 plus two length-prefixed strings) is what such a client parses; the short form
            // would misalign the body.
            //
            // The two strings are the anticheat info and the screenshot/detection info. With client
            // checks disabled and the anticheat not hidden they are "0" and "" (five bytes, not
            // four). The u16 ahead of them is the registry player count.
            ushort rosterVersion = session.NegotiatedBitStreamVersion != 0
                ? session.NegotiatedBitStreamVersion
                : bitStreamVersion;

            ushort viewVersion = rosterVersion;
            ushort playerVersion = rosterVersion;

            // `bJustJoined` is passed through by default: the roster broadcast to existing players
            // and the roster sent to the joiner legitimately differ, and Slipe already writes the
            // right value. `JustJoinedOverride` exists only so a run can force the bit.
            bool? justJoined = this.JustJoinedOverride;

            int bodyLength = data.Length;
            ushort registryValue = this.RegistryPlayerCount;
            data = MtaGamePackets.PrependPlayerListAcBlock(
                data,
                viewVersion,
                playerVersion,
                this.HideAntiCheatFromClient,
                registryValue: registryValue,
                acInfo: this.AcInfoBytes,
                justJoined: justJoined);

            this.Trace?.Invoke(
                $"PLAYER_LIST framed: {bodyLength} B body -> {data.Length} B (anticheat block) " +
                $"at view version 0x{viewVersion:X} / player version 0x{playerVersion:X}, " +
                $"hide={this.HideAntiCheatFromClient}, registry={registryValue}, " +
                $"ac=\"{this.AcInfoText}\"" +
                (justJoined is bool forced ? $", justJoined forced {forced}" : string.Empty));
        }

        // PLAYER_QUIT (mtaId 5, wire 0x68) carries the same registry u16 ahead of its game payload,
        // but gated on the packet's version alone (>= 0x4a), and it carries no anticheat strings.
        // Omitting it makes a modern client read the game body two bytes early.
        if (packetId == (PacketId)MtaGamePackets.PlayerQuitMtaId)
        {
            ushort quitVersion = session.NegotiatedBitStreamVersion != 0
                ? session.NegotiatedBitStreamVersion
                : bitStreamVersion;

            ushort registryValue = this.RegistryPlayerCount;
            int bodyLength = data.Length;
            data = MtaGamePackets.PrependPlayerQuitRegistryValue(data, quitVersion, registryValue);

            if (data.Length != bodyLength)
            {
                this.Trace?.Invoke(
                    $"PLAYER_QUIT framed: {bodyLength} B body -> {data.Length} B " +
                    $"(registry value {registryValue}) at view version 0x{quitVersion:X}");
            }
        }

        var payload = new byte[data.Length + 1];
        payload[0] = wireId;
        Array.Copy(data, 0, payload, 1, data.Length);

        this.SendWirePayload(session, payload, ToWireReliability(reliability));

        // `priority` has no wire representation: it only selects an internal send queue, not a
        // datagram field.
        _ = priority;
    }

    /// <summary>
    /// Sends one already-framed packet payload (wire id included), splitting it over as many
    /// datagrams as the size cap requires. Any queued acks ride on the first datagram only.
    /// </summary>
    private void SendWirePayload(MtaNetSession session, byte[] payload, WireReliability reliability)
    {
        ulong now = this.NowMilliseconds;
        OutgoingPacket[] packets;
        AckRange[] acks;
        ushort splitPacketId;

        // The sequence/ordering counters and PendingAcks are shared with the receive path and the
        // retransmission timer, so the whole build happens under the session's reliability lock;
        // otherwise a resend and a new send could interleave on NextOutboundSequenceNumber, and an
        // ack could drain a range a concurrent flush had already taken.
        lock (session.ReliabilitySyncRoot)
        {
            splitPacketId = session.NextSplitPacketId++;
            packets = RakNetDatagramWriter.Split(
                payload,
                reliability,
                splitPacketId,
                NextOrderingIndex(session, reliability),
                session.NextOutboundSequenceNumber,
                out uint nextSequenceNumber);
            session.NextOutboundSequenceNumber = nextSequenceNumber;

            acks = session.PendingAcks.Count == 0
                ? Array.Empty<AckRange>()
                : session.PendingAcks.ToArray();
            session.PendingAcks.Clear();

            // Only the reliable family is retransmitted, so only it is retained: resending an
            // unreliable packet would be a new packet rather than a repair.
            //
            // A split message is retained part by part. Each part has its own sequence number and
            // is acked independently, so losing one part resends one part, not the whole message.
            if (reliability.IsReliable())
            {
                foreach (OutgoingPacket packet in packets)
                {
                    // Bounded before the add, not after: a peer that elicits reliable replies and
                    // never acks must not be able to grow this ledger without limit.
                    if (session.UnackedReliable.Count >= MtaNetSession.MaxUnackedReliablePackets)
                        this.DropOldestUnackedReliable(session);

                    session.UnackedReliable[packet.SequenceNumber] =
                        new PendingReliablePacket(packet, now);
                }
            }
        }

        if (packets.Length > 1)
        {
            this.Trace?.Invoke(
                $"splitting {payload.Length} B payload into {packets.Length} datagrams (split id {splitPacketId})");
        }

        for (int i = 0; i < packets.Length; i++)
        {
            IReadOnlyList<AckRange> carried = i == 0 ? acks : Array.Empty<AckRange>();
            byte[] datagram = RakNetDatagramWriter.Build(now, carried, new[] { packets[i] });
            this.SendRaw(datagram, session.EndPoint);
        }
    }

    /// <summary>
    /// Drops the in-flight reliable packet that has been waiting longest, so the ledger can accept a
    /// new one without exceeding <see cref="MtaNetSession.MaxUnackedReliablePackets"/>.
    ///
    /// <para>
    /// The dropped packet is simply never retransmitted, the same outcome as exhausting
    /// <see cref="RetransmissionPolicy.MaximumRetransmissions"/> but reached by count instead of by
    /// clock. Oldest-by-send-time is used rather than lowest sequence number because the sequence
    /// number is a wrapping 32-bit counter. Called under the session's reliability lock.
    /// </para>
    /// </summary>
    private void DropOldestUnackedReliable(MtaNetSession session)
    {
        uint oldest = 0;
        ulong oldestAt = ulong.MaxValue;
        bool found = false;

        foreach (PendingReliablePacket pending in session.UnackedReliable.Values)
        {
            if (found && pending.LastSentAtMilliseconds >= oldestAt)
                continue;

            oldestAt = pending.LastSentAtMilliseconds;
            oldest = pending.Packet.SequenceNumber;
            found = true;
        }

        if (!found)
            return;

        session.UnackedReliable.Remove(oldest);
        this.Trace?.Invoke(
            $"in-flight reliable ledger full for {session.EndPoint}: dropped seq={oldest} unsent");
    }

    private static uint NextOrderingIndex(MtaNetSession session, WireReliability reliability)
    {
        if (!reliability.CarriesOrdering())
            return 0;

        // Channel 0 unconditionally: every server-originated packet observed uses channel 0, and
        // non-zero channels appear only client-to-server. The dictionary is keyed so a non-zero
        // channel can be added if one is ever observed.
        //
        // Two index spaces, not one: reliability 1 and 4 (sequenced) draw from one per-channel
        // counter array and reliability 3 (ordered) from another, and the receive side keeps one
        // expected index per channel per family. Drawing both from one counter would spend ordered
        // indices on sequenced traffic, so the peer's ordered channel would wait forever for an
        // index already used elsewhere and stop delivering reliable packets.
        Dictionary<byte, uint> counters = reliability == WireReliability.ReliableOrdered
            ? session.NextOrderedIndex
            : session.NextSequencedIndex;
        counters.TryGetValue(0, out uint index);
        counters[0] = index + 1;
        return index;
    }

    /// <summary>
    /// Slipe's <c>PacketReliability</c> and this port's <see cref="WireReliability"/> share the same
    /// numeric values; only the names at 3 and 4 differ.
    /// </summary>
    private static WireReliability ToWireReliability(PacketReliability reliability) => reliability switch
    {
        PacketReliability.Unreliable => WireReliability.Unreliable,
        PacketReliability.UnreliableSequenced => WireReliability.UnreliableSequenced,
        PacketReliability.Reliable => WireReliability.Reliable,
        PacketReliability.ReliableSequenced => WireReliability.ReliableOrdered,
        PacketReliability.ReliableSequencedCanDropPackets => WireReliability.ReliableSequenced,
        _ => WireReliability.ReliableOrdered,
    };

    /// <summary>
    /// Every behavioural time read in the wrapper funnels through here: split reassembly, ordering,
    /// ack flushes, outbound datagram stamps and the offline 0x26 reply all use the injected clock.
    /// </summary>
    private ulong NowMilliseconds => this.clock.NowMilliseconds;

    private void SendRaw(byte[] datagram, IPEndPoint remote)
    {
        this.Capture?.Record(
            CaptureDirection.Tx, remote, datagram, datagram.Length, CaptureDescribe.Outbound(datagram, datagram.Length));

        try
        {
            this.socket?.SendTo(datagram, remote);
        }
        catch (SocketException exception)
        {
            this.Trace?.Invoke($"send to {remote} failed: {exception.SocketErrorCode}");
        }
        catch (ObjectDisposedException)
        {
        }
    }

    // ---------------------------------------------------------------- session metadata

    public void SetVersion(ulong binaryAddress, ushort version)
    {
        if (this.sessions.TryGetValue(binaryAddress, out MtaNetSession? session))
            session.NegotiatedBitStreamVersion = version;
    }

    public Tuple<string, string, string> GetClientSerialExtraAndVersion(ulong binaryAddress) =>
        this.sessions.TryGetValue(binaryAddress, out MtaNetSession? session)
            ? Tuple.Create(session.Serial ?? string.Empty, session.Extra ?? string.Empty, session.ClientVersion ?? string.Empty)
            : Tuple.Create(string.Empty, string.Empty, string.Empty);

    public IPAddress GetPlayerIp(ulong binaryAddress) =>
        this.sessions.TryGetValue(binaryAddress, out MtaNetSession? session)
            ? session.EndPoint.Address
            : IPAddress.None;

    /// <summary>
    /// The session for <paramref name="binaryAddress"/>, or <c>null</c>. A test seam for declaring
    /// per-channel ordering preconditions that no wire byte can express.
    /// </summary>
    internal MtaNetSession? FindSession(ulong binaryAddress) =>
        this.sessions.TryGetValue(binaryAddress, out MtaNetSession? session) ? session : null;

    /// <summary>
    /// Re-sending the client's mod-file manifest is a no-op: mods are not tracked.
    /// </summary>
    public void ResendModPackets(ulong binaryAddress)
    {
        this.Trace?.Invoke($"ResendModPackets(0x{binaryAddress:X16}) -> no-op in the POC");
    }

    /// <summary>A no-op like <see cref="ResendModPackets"/>: there is no anti-cheat info stream.</summary>
    public void ResendPlayerACInfo(ulong binaryAddress)
    {
        this.Trace?.Invoke($"ResendPlayerACInfo(0x{binaryAddress:X16}) -> no-op in the POC");
    }

    // ---------------------------------------------------------------- anti-cheat configuration

    /// <summary>Last anti-cheat configuration handed down by Slipe Server.</summary>
    public IReadOnlyList<AntiCheat> DisabledAntiCheats { get; private set; } = Array.Empty<AntiCheat>();

    public bool HideAntiCheatFromClient { get; private set; }

    public AllowGta3ImgMods AllowGta3ImgMods { get; private set; } = AllowGta3ImgMods.None;

    public IReadOnlyList<SpecialDetection> EnabledSpecialDetections { get; private set; } = Array.Empty<SpecialDetection>();

    public DataFile DisallowedDataFiles { get; private set; } = DataFile.None;

    /// <summary>
    /// The <c>PLAYER_LIST</c> anticheat block's first string: the client-checks flag formatted as a
    /// decimal integer. Settable so a run can try alternative block shapes against a live client; the
    /// default is <see cref="MtaGamePackets.DefaultAcInfoText"/>. See
    /// <see cref="MtaGamePackets.PrependPlayerListAcBlock"/>.
    /// </summary>
    public string AcInfoText { get; set; } = MtaGamePackets.DefaultAcInfoText;

    /// <summary><see cref="AcInfoText"/> as the raw bytes written on the wire.</summary>
    public byte[] AcInfoBytes => Encoding.UTF8.GetBytes(this.AcInfoText);

    /// <summary>
    /// The block's second string. Empty by default, which is what a server with no special-detection
    /// map sends.
    /// </summary>
    public string SdInfoText { get; set; } = MtaGamePackets.DefaultSdInfoText;

    /// <summary>
    /// Diagnostic override for <c>bJustJoined</c>, the mod-level body's first bit
    /// (<c>WriteBit(m_bShowInChat)</c>). <c>null</c> (the default) leaves Slipe's own value untouched,
    /// which is correct: the roster broadcast to existing players has <c>showInChat = true</c> and the
    /// joiner's own roster has <c>false</c>. Setting it forces that bit on every <c>PLAYER_LIST</c>;
    /// forcing <c>false</c> makes the client print its "Server AC info" line, the only client-side
    /// proof of where its read cursor lands after the two anticheat strings.
    /// </summary>
    public bool? JustJoinedOverride { get; set; }

    public void SetAntiCheatConfig(
        IEnumerable<AntiCheat> disabledAntiCheats,
        bool hideAntiCheatFromClient,
        AllowGta3ImgMods allowGta3ImgMods,
        IEnumerable<SpecialDetection> enabledSpecialDetections,
        DataFile disallowedDataFiles)
    {
        // No anti-cheat is enforced here; the values are retained so the join options can carry them.
        this.DisabledAntiCheats = new List<AntiCheat>(disabledAntiCheats ?? Array.Empty<AntiCheat>());
        this.HideAntiCheatFromClient = hideAntiCheatFromClient;
        this.AllowGta3ImgMods = allowGta3ImgMods;
        this.EnabledSpecialDetections = new List<SpecialDetection>(
            enabledSpecialDetections ?? Array.Empty<SpecialDetection>());
        this.DisallowedDataFiles = disallowedDataFiles;

        this.Trace?.Invoke(
            $"SetAntiCheatConfig: {this.DisabledAntiCheats.Count} disabled, "
            + $"{this.EnabledSpecialDetections.Count} special, datafiles={this.DisallowedDataFiles}, "
            + $"imgMods={this.AllowGta3ImgMods}, hide={this.HideAntiCheatFromClient}");
    }
}
