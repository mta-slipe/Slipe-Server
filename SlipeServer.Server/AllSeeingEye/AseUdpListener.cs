using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SlipeServer.Server.AllSeeingEye;

/// <summary>
/// Handles ASE queries on the game port + 123.
/// </summary>
public class AseUdpListener
{
    private const int cacheTime = 10 * 1000;
    private const int asePortOffset = 123;

    private readonly IAseQueryService aseQueryService;
    private readonly ILogger logger;
    private readonly HashSet<IPAddress> blockedIpAddresses;

    private readonly Cache<byte[]> fullCache;
    private readonly Cache<byte[]> lightCache;
    private readonly Cache<byte[]> xFireCache;

    public AseUdpListener(
        IAseQueryService aseQueryService,
        ILogger logger,
        ushort port,
        bool isDebug,
        IEnumerable<IPAddress> blockedIpAddresses)
    {
        if (port <= asePortOffset)
        {
            throw new ArgumentOutOfRangeException(
                nameof(port),
                port,
                "The ASE port must be greater than 123.");
        }

        this.aseQueryService = aseQueryService;
        this.logger = logger;
        this.blockedIpAddresses = new HashSet<IPAddress>(
            blockedIpAddresses);

        // The listener uses the ASE port, but responses contain the game port.
        ushort gamePort = checked((ushort)(port - asePortOffset));

        this.lightCache = new Cache<byte[]>(
            () => this.aseQueryService.QueryLight(
                gamePort,
                isDebug
                    ? Enums.VersionType.Custom
                    : Enums.VersionType.Release),
            cacheTime);

        this.xFireCache = new Cache<byte[]>(
            () => this.aseQueryService.QueryXFireLight(gamePort),
            cacheTime);

        this.fullCache = new Cache<byte[]>(
            () => this.aseQueryService.QueryFull(gamePort),
            cacheTime);

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
                this.logger.LogTrace(
                    "Blocked ASE request from {ipAddress}",
                    source.Address);

                return;
            }

            if (message.Length == 0)
                return;

            AseQueryType queryType = (AseQueryType)message[0];

            this.logger.LogTrace(
                "ASE request received for query type {aseQueryType}",
                queryType);

            byte[] data = queryType switch
            {
                AseQueryType.Full => this.fullCache.Get(),
                AseQueryType.Light => this.lightCache.Get(),
                AseQueryType.LightRelease => this.lightCache.Get(),
                AseQueryType.XFire => this.xFireCache.Get(),
                AseQueryType.Version => this.aseQueryService.GetVersion().Select(c => (byte)c).ToArray(),
                _ => throw new NotImplementedException($"'{message[0]}' is not a valid ASE query"),
            } ?? Array.Empty<byte>();

            socket.Send(data, data.Length, source);
        }
        catch (ObjectDisposedException)
        {
            // The listener socket has been closed.
        }
        catch (Exception exception)
        {
            this.logger.LogError(
                exception,
                "ASE request failed: {ExceptionDetails}",
                exception.ToString());
        }
        finally
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
                this.logger.LogError(
                    exception,
                    "Unable to resume receiving ASE requests");
            }
        }
    }

    private void StartListening(ushort port)
    {
        var socket = new UdpClient(port);

        try
        {
            socket.BeginReceive(OnUdpData, socket);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
}
