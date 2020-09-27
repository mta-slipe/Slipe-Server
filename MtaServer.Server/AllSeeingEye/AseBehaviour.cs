using Microsoft.Extensions.Logging;
using MtaServer.Server.Elements;
using MtaServer.Server.Extensions;
using MtaServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace MtaServer.Server.AllSeeingEye
{
    public class AseBehaviour
    {
        private const int cacheTime = 10 * 1000;
        private readonly IAseQueryService aseQueryService;
        private readonly ILogger logger;

        private readonly Cache<byte[]> fullCache;
        private readonly Cache<byte[]> lightCache;
        private readonly Cache<byte[]> xFireCache;
        private readonly AseVersion aseVersion;
        private readonly Dictionary<string, string> rules = new Dictionary<string, string>();

        public AseBehaviour(IAseQueryService aseQueryService, Configuration configuration, ILogger logger)
        {
            this.aseQueryService = aseQueryService;
            this.logger = logger;

            aseVersion = AseVersion.v1_5;

            lightCache = new Cache<byte[]>(() => aseQueryService.QueryLight(), cacheTime);
            xFireCache = new Cache<byte[]>(() => aseQueryService.QueryXFireLight(), cacheTime);
            fullCache = new Cache<byte[]>(() => aseQueryService.QueryFull(), cacheTime);

            StartListening((ushort)(configuration.Port + 123));
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
                    IPEndPoint source = new IPEndPoint(0, 0);
                    byte[] message = socket.EndReceive(result, ref source);
                    byte[] data;

                    AseQueryType queryType = (AseQueryType)(message[0]);

                    this.logger.LogInformation($"ASE request received for query type {queryType}");
                    switch (queryType)
                    {
                        case AseQueryType.Full:
                            data = fullCache.Get();
                            break;
                        case AseQueryType.Light:
                            data = lightCache.Get();
                            break;
                        case AseQueryType.LightRelease:
                            data = lightCache.Get();
                            break;
                        case AseQueryType.XFire:
                            data = xFireCache.Get();
                            break;
                        case AseQueryType.Version:
                            data = this.aseQueryService.GetVersion().Select(c => (byte)c).ToArray();
                            break;
                        default:
                            throw new NotImplementedException($"'{message[0]}' is not a valid ASE query");
                    }

                    socket.Send(data, data.Length, source);
                    socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
                }
                catch (Exception e)
                {
                    this.logger.LogError(e.Message);
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
}
