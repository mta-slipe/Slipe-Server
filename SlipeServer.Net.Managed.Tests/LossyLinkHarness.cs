using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SlipeServer.Net.Managed;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>What the link does with one datagram offered to it.</summary>
internal readonly record struct LinkAction(bool Drop, int Copies, int DelayMilliseconds)
{
    /// <summary>Deliver once, now.</summary>
    internal static LinkAction Pass => new(false, 1, 0);

    /// <summary>Lose it. Nothing reaches the far end and nothing is retried by the link itself.</summary>
    internal static LinkAction Discard => new(true, 0, 0);

    /// <summary>
    /// Deliver once, this many milliseconds later. The clock is manual, so the delay costs no wall
    /// time: it only moves the datagram into the future of <see cref="ManualClock"/>.
    /// </summary>
    internal static LinkAction Hold(int milliseconds) => new(false, 1, milliseconds);

    /// <summary>Deliver <paramref name="copies"/> times, now.</summary>
    internal static LinkAction Duplicate(int copies = 2) => new(false, copies, 0);
}

/// <summary>
/// One datagram in flight between the two ends, with its packets decoded by
/// <see cref="ReferenceDatagram"/> so a drop decision can be made on what the bytes actually carry
/// rather than on what the port believes it sent.
/// </summary>
internal sealed class LinkDatagram
{
    private IReadOnlyList<ReferencePacket>? packets;

    internal LinkDatagram(bool fromClient, byte[] bytes)
    {
        this.FromClient = fromClient;
        this.Bytes = bytes;
    }

    /// <summary>True when the client end sent it, i.e. it is travelling client-to-server.</summary>
    internal bool FromClient { get; }

    internal byte[] Bytes { get; }

    /// <summary>
    /// The packets the datagram carries, decoded by the independent reference reader. An ack-only
    /// datagram carries none.
    /// </summary>
    internal IReadOnlyList<ReferencePacket> Packets =>
        this.packets ??= ReferenceDatagram.TryParse(this.Bytes, out ReferenceParsedDatagram? parsed, out _) &&
                         parsed is not null
            ? parsed.Packets
            : Array.Empty<ReferencePacket>();
}

/// <summary>
/// A deterministic lossy link between two headless <see cref="MtaNetWrapper"/> instances.
///
/// <para>
/// Each end is a real wrapper with a real session record and a real capture file, primed with the
/// recorded handshake exactly as <see cref="WrapperHarness.ConnectSession"/> does. What sits between
/// them is this class: it harvests each end's outbound datagrams out of its
/// <see cref="PacketCapture"/>, offers every one to a policy, and feeds the survivors into the other
/// end through the public <see cref="MtaNetWrapper.InjectDatagram"/> seam — the same entry point the
/// UDP receive loop uses.
/// </para>
///
/// <para>
/// Nothing here waits. Time only moves when a test calls <see cref="Advance"/>, which advances the
/// shared <see cref="ManualClock"/>, runs both ends' retransmission scans, and then settles the
/// link. Every outcome is therefore a function of the policy and the advance sequence alone: the same
/// test run twice produces the same bytes in the same order, and there is no scheduler to race.
/// </para>
///
/// <para>
/// Datagrams are delivered in the order they were harvested, and a delayed datagram keeps its place
/// among the datagrams that share its delivery instant, so "reorder" is expressed as "hold one of
/// them", not as nondeterminism.
/// </para>
/// </summary>
internal sealed class LossyLinkHarness : IDisposable
{
    /// <summary>The endpoint the client end presents to the server end.</summary>
    internal static readonly IPEndPoint ClientEndPoint = WrapperHarness.CaptureLogPeer;

    /// <summary>The endpoint the server end presents to the client end.</summary>
    internal static readonly IPEndPoint ServerEndPoint = WireTestKit.Endpoint("192.0.2.31:50666");

    /// <summary>Upper bound on settle rounds, so a policy that keeps generating traffic fails loudly.</summary>
    private const int MaximumPumpRounds = 512;

    private readonly List<Queued> queue = new();
    private readonly int clientPrimed;
    private readonly int serverPrimed;
    private int clientCursor;
    private int serverCursor;
    private long insertionOrder;
    private bool disposed;

    internal LossyLinkHarness(int seed = 0)
    {
        this.Seed = seed;
        this.Clock = new ManualClock();
        this.Server = new WrapperHarness(clock: this.Clock);
        this.Client = new WrapperHarness(clock: this.Clock);

        // Both ends are taken through the recorded handshake so each has a connected session record.
        this.Server.ConnectSession(ClientEndPoint);
        this.Client.ConnectSession(ServerEndPoint);

        // The handshake itself produces traffic: each end answers the injected request with a reliable
        // packet of its own. Settle that through the link before the test starts, exactly as a real
        // connection would, so the reply is delivered, acknowledged, and retired. Otherwise a
        // handshake packet would sit in the ledger for the whole test and be retransmitted alongside
        // the traffic the test actually cares about.
        this.Pump();

        this.serverPrimed = this.Server.Tx.Count;
        this.clientPrimed = this.Client.Tx.Count;
        this.serverCursor = this.serverPrimed;
        this.clientCursor = this.clientPrimed;

        // Priming traffic is setup, not test traffic: it stays out of the log a test reads.
        this.Log.Clear();

        if (this.ServerSession().UnackedReliableCount != 0 || this.ClientSession().UnackedReliableCount != 0)
        {
            throw new InvalidOperationException(
                "the handshake did not settle: " +
                $"{this.ServerSession().UnackedReliableCount} server and " +
                $"{this.ClientSession().UnackedReliableCount} client packet(s) are still unacknowledged");
        }
    }

    /// <summary>Kept so a failing run can be reproduced; the policy is what actually decides.</summary>
    internal int Seed { get; }

    internal ManualClock Clock { get; }

    /// <summary>The end that sends game packets.</summary>
    internal WrapperHarness Server { get; }

    /// <summary>The end that receives them and answers with acks.</summary>
    internal WrapperHarness Client { get; }

    /// <summary>
    /// Decides what happens to one datagram. Unset means an ideal link. The policy must be a pure
    /// function of the datagram and its own captured state; it must not consult a clock.
    /// </summary>
    internal Func<LinkDatagram, LinkAction>? Policy { get; set; }

    /// <summary>One line per datagram offered to the link, in offer order.</summary>
    internal List<string> Log { get; } = new();

    /// <summary>Datagrams accepted by the link but not yet delivered (held for a delay).</summary>
    internal int QueuedCount => this.queue.Count;

    /// <summary>The binary address the server end uses for the client.</summary>
    internal ulong ServerAddress => MtaNetWrapper.BinaryAddressOf(ClientEndPoint);

    /// <summary>The binary address the client end uses for the server.</summary>
    internal ulong ClientAddress => MtaNetWrapper.BinaryAddressOf(ServerEndPoint);

    /// <summary>Every datagram the server has put on the wire since the pair was built, resends included.</summary>
    internal IReadOnlyList<CaptureLine> ServerTx => this.Server.Tx.Skip(this.serverPrimed).ToList();

    /// <summary>Every datagram the client has put on the wire since the pair was built, resends included.</summary>
    internal IReadOnlyList<CaptureLine> ClientTx => this.Client.Tx.Skip(this.clientPrimed).ToList();

    internal MtaNetSession ServerSession()
    {
        MtaNetSession? session = this.Server.Wrapper.FindSession(this.ServerAddress);
        return session ?? throw new InvalidOperationException($"the server end has no session for {ClientEndPoint}");
    }

    internal MtaNetSession ClientSession()
    {
        MtaNetSession? session = this.Client.Wrapper.FindSession(this.ClientAddress);
        return session ?? throw new InvalidOperationException($"the client end has no session for {ServerEndPoint}");
    }

    /// <summary>
    /// Sends one game packet from the server end and settles the link, so the caller sees the result
    /// of one complete round trip at the current instant.
    /// </summary>
    internal void Send(
        PacketId id,
        byte[] body,
        PacketReliability reliability = PacketReliability.ReliableSequenced,
        ushort bitStreamVersion = 0x90)
    {
        this.Server.Wrapper.SendPacket(
            this.ServerAddress, id, bitStreamVersion, body, reliability: reliability);

        this.Pump();
    }

    /// <summary>
    /// Moves the clock forward by <paramref name="milliseconds"/>, runs both ends' retransmission
    /// scans, and settles the link. Returns how many datagrams were retransmitted.
    /// </summary>
    internal int Advance(ulong milliseconds)
    {
        this.Clock.Advance(milliseconds);

        int resent = this.Server.Wrapper.TickRetransmissions() + this.Client.Wrapper.TickRetransmissions();

        this.Pump();

        return resent;
    }

    /// <summary>
    /// Harvests whatever each end has put on the wire, offers it to the policy, and delivers
    /// everything the link currently owes. Repeats until neither end produces anything new, so one
    /// call settles a whole exchange.
    /// </summary>
    internal int Pump()
    {
        int delivered = 0;

        for (int round = 0; round < MaximumPumpRounds; round++)
        {
            bool progress = this.Harvest(this.Server, fromClient: false);
            progress |= this.Harvest(this.Client, fromClient: true);

            int due = this.DeliverDue();
            delivered += due;
            progress |= due > 0;

            if (!progress)
                return delivered;
        }

        throw new InvalidOperationException(
            $"the link did not settle after {MaximumPumpRounds} rounds; {this.queue.Count} datagram(s) still queued");
    }

    /// <summary>
    /// Decodes one captured datagram with the independent reference reader, never with the port's own
    /// parser, so an assertion about what went on the wire does not assume the writer is correct.
    /// </summary>
    internal static ReferenceParsedDatagram Parse(CaptureLine line) =>
        ReferenceDatagram.TryParse(line.Bytes, out ReferenceParsedDatagram? parsed, out string error) &&
        parsed is not null
            ? parsed
            : throw new InvalidOperationException($"capture line {line.Seq} did not decode: {error}");

    public void Dispose()
    {
        if (this.disposed)
            return;

        this.disposed = true;

        this.Server.Dispose();
        this.Client.Dispose();
    }

    private bool Harvest(WrapperHarness end, bool fromClient)
    {
        IReadOnlyList<CaptureLine> tx = end.Tx;
        int cursor = fromClient ? this.clientCursor : this.serverCursor;

        if (tx.Count <= cursor)
            return false;

        for (int i = cursor; i < tx.Count; i++)
            this.Offer(new LinkDatagram(fromClient, tx[i].Bytes));

        if (fromClient)
            this.clientCursor = tx.Count;
        else
            this.serverCursor = tx.Count;

        return true;
    }

    private void Offer(LinkDatagram datagram)
    {
        LinkAction action = this.Policy?.Invoke(datagram) ?? LinkAction.Pass;

        this.Log.Add(
            $"{(datagram.FromClient ? "C>S" : "S>C")} {datagram.Bytes.Length}B " +
            $"[{string.Join(",", datagram.Packets.Select(packet => packet.Sequence))}] " +
            (action.Drop ? "-> drop" : $"-> {action.Copies} cop(ies), +{action.DelayMilliseconds}ms"));

        if (action.Drop)
            return;

        for (int i = 0; i < action.Copies; i++)
        {
            this.queue.Add(new Queued(
                datagram,
                this.Clock.NowMilliseconds + (ulong)Math.Max(0, action.DelayMilliseconds),
                this.insertionOrder++));
        }
    }

    private int DeliverDue()
    {
        if (this.queue.Count == 0)
            return 0;

        ulong now = this.Clock.NowMilliseconds;

        var due = new List<Queued>();
        var held = new List<Queued>(this.queue.Count);
        foreach (Queued queued in this.queue)
        {
            if (queued.DeliverAt <= now)
                due.Add(queued);
            else
                held.Add(queued);
        }

        if (due.Count == 0)
            return 0;

        this.queue.Clear();
        this.queue.AddRange(held);

        due.Sort((left, right) => left.DeliverAt != right.DeliverAt
            ? left.DeliverAt.CompareTo(right.DeliverAt)
            : left.Order.CompareTo(right.Order));

        foreach (Queued queued in due)
        {
            if (queued.Datagram.FromClient)
                this.Server.InjectRaw(queued.Datagram.Bytes, ClientEndPoint);
            else
                this.Client.InjectRaw(queued.Datagram.Bytes, ServerEndPoint);
        }

        return due.Count;
    }

    private sealed record Queued(LinkDatagram Datagram, ulong DeliverAt, long Order);
}
