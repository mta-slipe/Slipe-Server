using Microsoft.Extensions.Logging;
using SlipeServer.Net.Wrappers;
using SlipeServer.Net.Wrappers.Enums;
using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SlipeServer.Server.AllSeeingEye;

/// <summary>
/// Handles ASE queries on the game port + 123.
/// </summary>
public class AseUdpListener : IDisposable
{
    private const int cacheTime = 10 * 1000;
    internal const int AsePortOffset = 123;

    /// <summary>
    /// How long a version-bearing query is held while its peer's build is still unknown. A client
    /// sends its join data within milliseconds of the probe and gives up on the probe after two
    /// seconds, so this is long enough to learn the build and far short of the deadline.
    /// </summary>
    public static readonly TimeSpan HintHoldDuration = TimeSpan.FromMilliseconds(500);

    private const int holdSweepMilliseconds = 25;

    private readonly IAseQueryService aseQueryService;
    private readonly ILogger logger;
    private readonly HashSet<IPAddress> blockedIpAddresses;
    private readonly IPeerVersionHints? peerVersionHints;
    private readonly MtaPeerBuildType defaultBuild;

    private readonly Cache<byte[]> fullCache;
    private readonly Cache<byte[]> releaseLightCache;
    private readonly Cache<byte[]> debugLightCache;
    private readonly Cache<byte[]> xFireCache;
    private readonly AsePendingReplies? pendingReplies;
    private readonly Timer? holdTimer;

    private UdpClient? socket;
    private volatile bool disposed;

    /// <param name="port">The ASE port, which is the game port + 123.</param>
    /// <param name="peerVersionHints">
    /// When supplied, a light query from a peer whose build is not yet known is held for
    /// <see cref="HintHoldDuration"/> so the reply can advertise that peer's version family instead
    /// of <paramref name="isDebug"/>'s. Omit to always answer with the listener's fixed family.
    /// </param>
    public AseUdpListener(IAseQueryService aseQueryService, ILogger logger, ushort port, bool isDebug, IEnumerable<IPAddress> blockedIpAddresses, IPeerVersionHints? peerVersionHints = null)
        : this(aseQueryService, logger, port, isDebug, blockedIpAddresses, peerVersionHints, null)
    {
    }

    /// <param name="clock">
    /// Monotonic milliseconds for the hold queue. Omit for <see cref="Environment.TickCount64"/>; a
    /// test passes a manual clock so a hold can be expired without sleeping.
    /// </param>
    internal AseUdpListener(IAseQueryService aseQueryService, ILogger logger, ushort port, bool isDebug, IEnumerable<IPAddress> blockedIpAddresses, IPeerVersionHints? peerVersionHints, Func<ulong>? clock)
    {
        if (port <= AsePortOffset)
        {
            throw new ArgumentOutOfRangeException(
                nameof(port),
                port,
                $"The ASE port must be greater than {AsePortOffset}.");
        }

        this.aseQueryService = aseQueryService;
        this.logger = logger;
        this.blockedIpAddresses = new HashSet<IPAddress>(blockedIpAddresses);
        this.peerVersionHints = peerVersionHints;
        this.defaultBuild = isDebug ? MtaPeerBuildType.Debug : MtaPeerBuildType.Release;

        // The listener uses the ASE port, but responses contain the game port.
        ushort gamePort = checked((ushort)(port - AsePortOffset));

        this.releaseLightCache = new Cache<byte[]>(() =>
            aseQueryService.QueryLight(gamePort, VersionType.Release)
        , cacheTime);
        this.debugLightCache = new Cache<byte[]>(() =>
            aseQueryService.QueryLight(gamePort, VersionType.Custom)
        , cacheTime);
        this.xFireCache = new Cache<byte[]>(() => aseQueryService.QueryXFireLight(gamePort), cacheTime);
        this.fullCache = new Cache<byte[]>(() => aseQueryService.QueryFull(gamePort), cacheTime);

        if (peerVersionHints != null)
        {
            this.pendingReplies = new AsePendingReplies(HintHoldDuration, clock);
            this.holdTimer = new Timer(_ => FlushExpiredReplies(), null, holdSweepMilliseconds, holdSweepMilliseconds);
        }

        StartListening(port);
    }

    public void SetRule(string key, string value) =>
        this.aseQueryService.SetRule(key, value);

    public bool RemoveRule(string key) =>
        this.aseQueryService.RemoveRule(key);

    public string? GetRule(string key) =>
        this.aseQueryService.GetRule(key);

    private void OnUdpData(IAsyncResult result)
    {
        if (result.AsyncState is not UdpClient socket)
            return;

        try
        {
            IPEndPoint? source = new IPEndPoint(IPAddress.Any, 0);
            byte[] message = socket.EndReceive(result, ref source);

            if (source is null)
                return;

            if (this.blockedIpAddresses.Contains(source.Address))
            {
                this.logger.LogTrace("Blocked ASE request from {ipAddress}", source.Address);
                return;
            }

            if (message.Length == 0)
                return;

            AseQueryType queryType = (AseQueryType)message[0];

            this.logger.LogTrace("ASE request received for query type {aseQueryType}", queryType);

            this.HandleQuery(queryType, source, data => socket.Send(data, data.Length, source));
        }
        catch (ObjectDisposedException)
        {
            // The listener socket has been closed.
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception, "ASE request failed: {ExceptionDetails}", exception.ToString());
        }
        finally
        {
            if (!this.disposed)
            {
                try
                {
                    socket.BeginReceive(OnUdpData, socket);
                }
                catch (ObjectDisposedException)
                {
                    // A closed socket cannot receive another request.
                }
                catch (Exception exception)
                {
                    this.logger.LogError(exception, "Unable to resume receiving ASE requests");
                }
            }
        }
    }

    /// <summary>
    /// Answers one query, holding a light query back when the peer's build is not yet known.
    /// </summary>
    internal void HandleQuery(AseQueryType queryType, IPEndPoint source, Action<byte[]> send)
    {
        if (this.peerVersionHints != null && IsVersionBearing(queryType))
        {
            if (this.peerVersionHints.TryGet(source.Address, out MtaPeerBuildType build))
            {
                send(this.GetQueryData(queryType, build));
                return;
            }

            this.logger.LogTrace(
                "Holding ASE query {aseQueryType} from {ipAddress} until the peer's build is known",
                queryType, source.Address);
            this.pendingReplies!.Enqueue(source, queryType);
            return;
        }

        send(this.GetQueryData(queryType, this.defaultBuild));
    }

    private void FlushExpiredReplies() =>
        FlushExpiredReplies((data, endpoint) => this.socket?.Send(data, data.Length, endpoint));

    /// <summary>
    /// Answers every held query whose hold has elapsed, using the build recorded since it was held
    /// and falling back to the listener's own family when the peer still has no hint.
    /// </summary>
    internal void FlushExpiredReplies(Action<byte[], IPEndPoint> send)
    {
        if (this.pendingReplies == null)
            return;

        foreach (PendingAseQuery pending in this.pendingReplies.TakeExpired())
        {
            if (this.disposed)
                return;

            try
            {
                MtaPeerBuildType build = this.defaultBuild;

                if (this.peerVersionHints != null && this.peerVersionHints.TryGet(pending.Source.Address, out MtaPeerBuildType hinted))
                {
                    build = hinted;
                }
                else
                {
                    this.logger.LogTrace(
                        "No build hint for {ipAddress}; answering ASE query {aseQueryType} with the listener's default",
                        pending.Source.Address, pending.QueryType);
                }

                send(this.GetQueryData(pending.QueryType, build), pending.Source);
            }
            catch (Exception e)
            {
                // One failed send must not drop the replies still held behind it.
                if (!this.disposed)
                    this.logger.LogError(e, "Sending a held ASE reply to {ipAddress} failed", pending.Source.Address);
            }
        }
    }

    /// <summary>How many queries are currently held awaiting their peer's build.</summary>
    internal int PendingReplyCount => this.pendingReplies?.Count ?? 0;

    private byte[] GetQueryData(AseQueryType queryType, MtaPeerBuildType build) => queryType switch
    {
        AseQueryType.Full => this.fullCache.Get(),
        AseQueryType.Light => this.GetLightCache(build).Get(),
        AseQueryType.LightRelease => this.GetLightCache(build).Get(),
        AseQueryType.XFire => this.xFireCache.Get(),
        AseQueryType.Version => this.aseQueryService.GetVersion().Select(c => (byte)c).ToArray(),
        _ => throw new NotImplementedException($"'{queryType}' is not a valid ASE query"),
    } ?? Array.Empty<byte>();

    private Cache<byte[]> GetLightCache(MtaPeerBuildType build) =>
        build == MtaPeerBuildType.Debug ? this.debugLightCache : this.releaseLightCache;

    /// <summary>
    /// Whether a query's reply carries the version string a connecting client compares against its
    /// own. The client's <c>CConnectManager</c> always probes with <c>'r'</c>; full and XFire
    /// queries come from the server browser, and <see cref="IAseQueryService.QueryFull"/> has no
    /// version parameter to vary.
    /// </summary>
    private static bool IsVersionBearing(AseQueryType queryType) =>
        queryType is AseQueryType.Light or AseQueryType.LightRelease;

    private void StartListening(ushort port)
    {
        this.socket = new UdpClient(port);

        try
        {
            this.socket.BeginReceive(OnUdpData, this.socket);
        }
        catch
        {
            this.socket.Dispose();
            this.socket = null;
            throw;
        }
    }

    public void Dispose()
    {
        if (this.disposed)
            return;

        this.disposed = true;

        // Wait for a flush already in progress so it cannot touch the socket after it is closed.
        if (this.holdTimer != null)
        {
            using ManualResetEvent timerStopped = new(false);
            this.holdTimer.Dispose(timerStopped);
            timerStopped.WaitOne();
        }

        this.socket?.Dispose();
        GC.SuppressFinalize(this);
    }
}
