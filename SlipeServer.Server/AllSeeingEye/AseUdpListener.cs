using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SlipeServer.Server.AllSeeingEye;

/// <summary>
/// UDP listeners responsible for handling ASE query requests
/// </summary>
public class AseUdpListener
{
    private const int cacheTime = 10 * 1000;
    private readonly IAseQueryService aseQueryService;
    private readonly ILogger logger;

    private readonly Cache<byte[]> fullCache;
    private readonly Cache<byte[]> lightCache;
    private readonly Cache<byte[]> xFireCache;
    private readonly Dictionary<string, string> rules = [];

    public AseUdpListener(IAseQueryService aseQueryService, ILogger logger, ushort port, bool isDebug)
    {
        this.aseQueryService = aseQueryService;
        this.logger = logger;

        this.lightCache = new Cache<byte[]>(() => 
            aseQueryService.QueryLight(port, isDebug ? Enums.VersionType.Custom : Enums.VersionType.Release)
        , cacheTime);
        this.xFireCache = new Cache<byte[]>(() => aseQueryService.QueryXFireLight(), cacheTime);
        this.fullCache = new Cache<byte[]>(() => aseQueryService.QueryFull(port), cacheTime);

        StartListening(port);
    }

    public void SetRule(string key, string value)
    {
        this.rules[key] = value;
    }

    public bool RemoveRule(string key) => this.aseQueryService.RemoveRule(key);

    public string? GetRule(string key) => this.aseQueryService.GetRule(key);

    private void OnUdpData(IAsyncResult result)
    {
        if (result.AsyncState is UdpClient socket)
        {
            try
            {
                IPEndPoint? source = new IPEndPoint(0, 0);
                byte[] message = socket.EndReceive(result, ref source);
                AseQueryType queryType = (AseQueryType)(message[0]);

                this.logger.LogTrace("ASE request received for query type {aseQueryType}", queryType);

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
                socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "ASE request failed");
            }
        }
    }

    private void StartListening(ushort port)
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

        UdpClient socket = new UdpClient(port);
        socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
    }
}
