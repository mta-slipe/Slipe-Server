using SlipeServer.Net.Wrappers.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace SlipeServer.Net.Wrappers;

/// <summary>
/// Short-lived, per-peer memory of which client build is connecting.
///
/// <para>
/// A client starts its RakNet connection before it sends the <c>'r'</c> ASE probe, and the build is
/// only stated in the join data that follows the connection handshake. This store is the seam
/// between the two: the net wrapper records what it reads from the join data, and the ASE listener
/// consults it to answer each peer with the version family that peer was compiled for.
/// </para>
///
/// <para>
/// Peers are keyed by address alone, because the ASE socket and the game socket have different
/// ports. Two clients behind one NAT therefore share an entry and the most recent join wins.
/// </para>
/// </summary>
public interface IPeerVersionHints
{
    /// <summary>Remembers the build a peer is running, replacing any previous hint for that address.</summary>
    void Record(IPAddress peer, MtaPeerBuildType build);

    /// <summary>Reads the build last recorded for a peer, if that hint has not expired.</summary>
    bool TryGet(IPAddress peer, out MtaPeerBuildType build);

    /// <summary>Discards every hint.</summary>
    void Clear();
}

/// <summary>
/// The in-memory <see cref="IPeerVersionHints"/>: one entry per peer, expiring after
/// <see cref="DefaultTimeToLive"/>.
/// </summary>
/// <remarks>
/// The lifetime only has to outlast the gap between a peer's ASE probe and its join data, which is a
/// few tens of milliseconds; it is generous so that a probe which arrives after the join data is
/// still answered correctly.
/// </remarks>
public sealed class PeerVersionHints : IPeerVersionHints
{
    /// <summary>How long a recorded hint stays readable.</summary>
    public static readonly TimeSpan DefaultTimeToLive = TimeSpan.FromSeconds(5);

    private readonly ConcurrentDictionary<IPAddress, Entry> hints = new();
    private readonly ulong timeToLiveMilliseconds;
    private readonly Func<ulong> clock;

    /// <summary>Creates a store with the <see cref="DefaultTimeToLive"/> lifetime and the system clock.</summary>
    public PeerVersionHints()
        : this(DefaultTimeToLive, null)
    {
    }

    /// <param name="timeToLive">How long a hint stays readable after it is recorded.</param>
    /// <param name="clock">
    /// Monotonic milliseconds. Omit for <see cref="Environment.TickCount64"/>; a test passes a
    /// manual clock so expiry can be reached without sleeping.
    /// </param>
    public PeerVersionHints(TimeSpan timeToLive, Func<ulong>? clock = null)
    {
        if (timeToLive <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeToLive), timeToLive,
                "A hint lifetime must be positive.");

        this.timeToLiveMilliseconds = (ulong)timeToLive.TotalMilliseconds;
        this.clock = clock ?? (() => (ulong)Environment.TickCount64);
    }

    public void Record(IPAddress peer, MtaPeerBuildType build)
    {
        ArgumentNullException.ThrowIfNull(peer);
        this.hints[peer] = new Entry(build, this.clock() + this.timeToLiveMilliseconds);
    }

    public bool TryGet(IPAddress peer, out MtaPeerBuildType build)
    {
        build = default;

        if (peer is null || !this.hints.TryGetValue(peer, out Entry entry))
            return false;

        if (entry.ExpiresAt <= this.clock())
        {
            // Only drop the entry we actually read; a concurrent Record may already have replaced it.
            this.hints.TryRemove(new KeyValuePair<IPAddress, Entry>(peer, entry));
            return false;
        }

        build = entry.Build;
        return true;
    }

    public void Clear() => this.hints.Clear();

    private readonly record struct Entry(MtaPeerBuildType Build, ulong ExpiresAt);
}
