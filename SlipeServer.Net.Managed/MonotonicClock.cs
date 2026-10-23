namespace SlipeServer.Net.Managed;

/// <summary>
/// The single seam through which the port reads behavioural time.
///
/// <para>
/// Milliseconds, monotonic, unsigned — the unit the rest of the port speaks:
/// <see cref="InboundOrdering.Accept"/>, <see cref="SplitReassembler.Accept"/> and
/// <see cref="RakNetDatagramWriter.Build"/> all take <c>ulong</c> milliseconds. Only differences are
/// meaningful; the epoch is arbitrary.
/// </para>
///
/// <para>
/// This deliberately does not use <c>System.TimeProvider</c>. Its monotonic API is
/// <c>long GetTimestamp()</c> with a separate <c>TimestampFrequency</c>, so every call site would
/// need a conversion back to the <c>ulong</c> milliseconds above, and the test double would pull in
/// <c>Microsoft.Extensions.TimeProvider.Testing</c> to replace a handful of lines. The clock is a
/// <see cref="MtaNetWrapper"/> implementation detail: it is not part of Slipe's
/// <see cref="SlipeServer.Net.Wrappers.INetWrapper"/> contract.
/// </para>
/// </summary>
public interface IMonotonicClock
{
    /// <summary>Monotonic milliseconds. Never runs backwards.</summary>
    ulong NowMilliseconds { get; }
}

/// <summary>
/// The production clock: <see cref="Environment.TickCount64"/>, which is monotonic and unaffected by
/// wall-clock adjustments. A test substitutes its own <see cref="IMonotonicClock"/> so that a timeout
/// boundary can be reached without sleeping.
/// </summary>
public sealed class SystemMonotonicClock : IMonotonicClock
{
    /// <summary>The process-wide instance; a wrapper that is not given a clock uses this one.</summary>
    public static SystemMonotonicClock Instance { get; } = new();

    private SystemMonotonicClock()
    {
    }

    public ulong NowMilliseconds => (ulong)Environment.TickCount64;
}
