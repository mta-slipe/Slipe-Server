namespace SlipeServer.Net.Managed;

/// <summary>
/// This port's outbound retransmission policy — the constants behind the reliable-packet ledger.
///
/// <para>
/// The native implementation's ack timeout compares the time since the last ack and the time
/// since the last datagram against a threshold. Its numeric value and any maximum retry count are
/// not known, and guessing them would be a fabricated match. The values below are therefore this
/// port's own choice; they are not a claim about the native implementation.
/// </para>
///
/// <para>
/// The timeout is <em>fixed</em> rather than RTT-estimated. That is a deliberate trade: a fixed
/// window makes every boundary reachable by advancing <see cref="IMonotonicClock"/>, so a test
/// asserts "exactly one millisecond before the timeout nothing is resent, at the timeout it is"
/// without a sleep, and the same numbers drive production. An estimator would add a second,
/// untestable source of timing variance for no benefit at MTA's packet rates.
/// </para>
///
/// <para>
/// Attempts are bounded. After <see cref="MaximumRetransmissions"/> resends the packet is abandoned
/// and a trace line names the give-up, so a blackholed peer cannot pin memory or spin the timer
/// forever.
/// </para>
/// </summary>
internal static class RetransmissionPolicy
{
    /// <summary>
    /// How often the wrapper's timer scans the ledger for overdue packets. Also the worst-case
    /// lateness of a resend, so it is kept well under <see cref="TimeoutMilliseconds"/>.
    /// </summary>
    internal const int TickMilliseconds = 100;

    /// <summary>
    /// Time from one transmission of a reliable packet to the next attempt. Fixed: no RTT estimate,
    /// no exponential backoff, because a fixed window is what makes the boundary assertable.
    /// </summary>
    internal const int TimeoutMilliseconds = 500;

    /// <summary>
    /// Resends allowed per reliable packet before it is abandoned. With
    /// <see cref="TimeoutMilliseconds"/> at 500 ms a packet is declared lost after roughly 3 s of
    /// unacknowledged transmission — long enough to ride out a burst, short enough that a dead peer
    /// is not retained indefinitely.
    /// </summary>
    internal const int MaximumRetransmissions = 5;
}
