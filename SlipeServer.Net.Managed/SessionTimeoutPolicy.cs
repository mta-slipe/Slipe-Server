namespace SlipeServer.Net.Managed;

/// <summary>
/// This port's dead-peer policy — the constants behind the silence sweep.
///
/// <para>
/// The native shape is a periodic sweep, at most once per second per peer, that compares each
/// peer's idle seconds against a configured per-peer threshold. That threshold is configuration,
/// not a fixed constant, so there is no native number to match. <see cref="TimeoutMilliseconds"/>
/// is therefore this port's own choice, as the retransmission numbers are; it is not a claim
/// about the native implementation.
/// </para>
///
/// <para>
/// 10 000 ms is chosen to agree with the port's two other silence budgets,
/// <c>InboundOrdering</c> and <c>SplitReassembler</c>. One number for "this peer has gone quiet" is
/// easier to reason about than three, and it is comfortably longer than any stall a live client
/// produces — a GC pause, an alt-tab, a NAT rebind — while still retiring a dead player within
/// roughly eleven seconds. The window is fixed rather than adaptive for the same reason as
/// <see cref="RetransmissionPolicy"/>: a fixed window makes both boundaries reachable by advancing
/// <see cref="IMonotonicClock"/>, so tests assert "one millisecond before the timeout the session is
/// still there, one millisecond past it the session is retired" with no sleep.
/// </para>
/// </summary>
internal static class SessionTimeoutPolicy
{
    /// <summary>
    /// How often the wrapper's timer scans the session table for silent peers, matching the native
    /// once-per-second sweep. Also the worst-case lateness of a retirement, so it is kept well under
    /// <see cref="TimeoutMilliseconds"/>.
    /// </summary>
    internal const int TickMilliseconds = 1000;

    /// <summary>
    /// How long a peer may go without a single accepted datagram before it is declared gone. Fixed:
    /// no adaptive estimate, because a fixed window is what makes the boundary assertable.
    /// </summary>
    internal const int TimeoutMilliseconds = 10_000;
}
