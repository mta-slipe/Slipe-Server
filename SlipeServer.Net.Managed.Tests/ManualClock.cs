namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// A monotonic clock the test moves by hand. Time only passes when a test calls
/// <see cref="Advance"/>, so a timeout boundary can be asserted exactly instead of with a sleep.
/// </summary>
internal sealed class ManualClock : IMonotonicClock
{
    private ulong now;

    internal ManualClock(ulong startMilliseconds = 0) => this.now = startMilliseconds;

    public ulong NowMilliseconds => this.now;

    internal void Advance(ulong milliseconds) => this.now += milliseconds;
}
