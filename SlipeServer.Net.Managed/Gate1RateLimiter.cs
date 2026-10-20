namespace SlipeServer.Net.Managed;

/// <summary>
/// Gate 1's per-IP rate window.
///
/// <para>
/// This is an instance rather than static state on purpose. The gate owns a mutable table, so a
/// process-wide table would make every <see cref="MtaNetWrapper"/> in the process share one rate
/// window: two servers, or two tests, would contaminate each other. Each wrapper owns one limiter,
/// created from the same injected <see cref="IMonotonicClock"/> the rest of the wrapper uses, so a
/// manual clock drives the window and a test can reach the sweep boundary without sleeping.
/// </para>
///
/// <para>
/// A non-zero result means "reject". Packets are keyed on the client IP rather than the port, the
/// limit applies only to mtaId 4 and 0x5a, at most 10,000 addresses are tracked, and the reject
/// threshold is <c>2 * Gate1RateLimit</c>.
/// </para>
/// </summary>
public sealed class Gate1RateLimiter
{
    /// <summary>Table bound: when full, the gate allows new addresses rather than growing.</summary>
    private const int MaxTrackedAddresses = 10_000;

    /// <summary>Stale windows are swept every 60,001 ms.</summary>
    private const ulong SweepMilliseconds = 60_001;

    private sealed class RateWindow
    {
        internal ulong WindowStartMilliseconds;
        internal int Count;
    }

    private readonly IMonotonicClock clock;
    private readonly Dictionary<uint, RateWindow> windows = new();
    private readonly object sync = new();
    private ulong lastSweepMilliseconds;

    public Gate1RateLimiter(IMonotonicClock? clock = null) =>
        this.clock = clock ?? SystemMonotonicClock.Instance;

    /// <summary>Addresses currently in the rate table. Exposed so a test can prove a sweep ran.</summary>
    internal int TrackedAddressCount
    {
        get
        {
            lock (this.sync)
                return this.windows.Count;
        }
    }

    /// <summary>
    /// Offers one inbound packet to the gate. Returns true when the packet is allowed.
    /// </summary>
    public bool Allow(uint ipDword, byte mtaId, MtaAdmissionOptions options)
    {
        if (options.Gate1RateLimit <= 0)
            return true;

        if (mtaId != 4 && mtaId != 0x5a)
            return true;

        ulong now = this.clock.NowMilliseconds;
        lock (this.sync)
        {
            if (Elapsed(now, this.lastSweepMilliseconds) >= (long)SweepMilliseconds)
            {
                this.lastSweepMilliseconds = now;
                var stale = new List<uint>();
                foreach (var (key, window) in this.windows)
                {
                    if (Elapsed(now, window.WindowStartMilliseconds) >= (long)SweepMilliseconds)
                        stale.Add(key);
                }

                foreach (var key in stale)
                    this.windows.Remove(key);
            }

            if (!this.windows.TryGetValue(ipDword, out RateWindow? entry))
            {
                if (this.windows.Count >= MaxTrackedAddresses)
                    return true;
                entry = new RateWindow { WindowStartMilliseconds = now };
                this.windows[ipDword] = entry;
            }

            // Banning is not implemented, so there is no ban flag or ban-until stamp to check.

            if (Elapsed(now, entry.WindowStartMilliseconds) >= options.Gate1WindowMs)
            {
                entry.WindowStartMilliseconds = now;
                entry.Count = 0;
            }

            entry.Count++;
            return entry.Count <= 2 * options.Gate1RateLimit;
        }
    }

    /// <summary>
    /// Elapsed time as a signed delta, so a negative window (never produced by the documented
    /// options) means "reset every time" rather than wrapping.
    /// </summary>
    private static long Elapsed(ulong now, ulong start) => unchecked((long)(now - start));
}
