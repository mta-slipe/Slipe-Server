using System;
using System.Collections.Generic;
using System.Net;

namespace SlipeServer.Server.AllSeeingEye;

/// <summary>An ASE query held back until its peer's build is known.</summary>
/// <param name="Source">The peer that sent the query.</param>
/// <param name="QueryType">The query byte, so the reply can be built once the build is known.</param>
/// <param name="Deadline">Monotonic milliseconds after which the reply falls back to the default.</param>
internal readonly record struct PendingAseQuery(IPEndPoint Source, AseQueryType QueryType, ulong Deadline);

/// <summary>
/// The queue of ASE queries held back so their reply can be tailored to the connecting client.
///
/// <para>
/// A client starts its RakNet connection before it sends the <c>'r'</c> ASE probe, so the probe
/// arrives ahead of the join data that states the build. Holding the reply lets the listener answer
/// with the version family that client was compiled for instead of a fixed one.
/// </para>
/// </summary>
internal sealed class AsePendingReplies
{
    /// <summary>Upper bound on held queries, so a query flood cannot grow the queue without limit.</summary>
    public const int MaxPending = 1024;

    private readonly List<PendingAseQuery> pending = [];
    private readonly ulong holdMilliseconds;
    private readonly Func<ulong> clock;

    /// <param name="holdDuration">How long a query may wait for its peer's build.</param>
    /// <param name="clock">Monotonic milliseconds. Omit for <see cref="Environment.TickCount64"/>.</param>
    public AsePendingReplies(TimeSpan holdDuration, Func<ulong>? clock = null)
    {
        if (holdDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(holdDuration), holdDuration,
                "A hold duration must be positive.");

        this.holdMilliseconds = (ulong)holdDuration.TotalMilliseconds;
        this.clock = clock ?? (() => (ulong)Environment.TickCount64);
    }

    /// <summary>How many queries are currently held.</summary>
    public int Count
    {
        get
        {
            lock (this.pending)
                return this.pending.Count;
        }
    }

    /// <summary>Holds a query for <paramref name="source"/>. The oldest entry is dropped when full.</summary>
    public void Enqueue(IPEndPoint source, AseQueryType queryType)
    {
        lock (this.pending)
        {
            if (this.pending.Count >= MaxPending)
                this.pending.RemoveAt(0);

            this.pending.Add(new PendingAseQuery(source, queryType, this.clock() + this.holdMilliseconds));
        }
    }

    /// <summary>Removes and returns every query whose hold has elapsed, oldest first.</summary>
    public IReadOnlyList<PendingAseQuery> TakeExpired()
    {
        lock (this.pending)
        {
            ulong now = this.clock();
            List<PendingAseQuery>? expired = null;

            for (int i = this.pending.Count - 1; i >= 0; i--)
            {
                if (this.pending[i].Deadline > now)
                    continue;

                (expired ??= []).Add(this.pending[i]);
                this.pending.RemoveAt(i);
            }

            if (expired == null)
                return Array.Empty<PendingAseQuery>();

            expired.Reverse();
            return expired;
        }
    }
}
