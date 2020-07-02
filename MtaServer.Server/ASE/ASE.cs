using MtaServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MtaServer.Server.ASE
{
    public enum EAseVersion
    {
        v1_5, // release build
        v1_5n, // debug build
    }

    public enum EBuildType
    {
        Custom = 0x01,
        Experimental = 0x03,
        Unstable = 0x05,
        Untested = 0x07,
        Release = 0x09
    }

    public class ASE
    {
        private ushort Port { get; }
        internal MtaServer MtaServer { get; }
        internal EAseVersion AseVersion { get; }
        internal EBuildType BuildType { get; }

        private byte[] LightCache { set; get; }
        private long LightCacheTime { set; get; } = 0;
        public ASE(MtaServer mtaServer)
        {
            this.Port = mtaServer.Configuration.Port;
            this.MtaServer = mtaServer;
            AseVersion = EAseVersion.v1_5;
            BuildType = EBuildType.Release;
            StartListening((ushort)(this.Port + 123));
        }

        private byte[] QueryLightCached()
        {
            if(LightCacheTime < DateTime.Now.Ticks)
            {
                LightCacheTime = DateTime.Now.Ticks + 15 * 1000 * 10000;
                LightCache = QueryLight();
            }
            return LightCache;
        }

        private byte[] QueryLight()
        {
            string aseVersion;
            switch(AseVersion)
            {
                case EAseVersion.v1_5:
                    aseVersion = "1.5";
                    break;
                case EAseVersion.v1_5n:
                    aseVersion = "1.5n";
                    break;
                default:
                    throw new NotImplementedException(AseVersion.ToString());
            }

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    List<string> playerNames = MtaServer.ElementRepository.GetByType<Player>(ElementType.Player).Select(o => o.NameNoColor).ToList();

                    int playersCount = playerNames.Count();
                    string strPlayerCount = playersCount + "/" + MtaServer.Configuration.MaxPlayers;
                    string buildType = $"{(byte)(VersionType.Release)} ";
                    string buildNumber = $"{(byte)BuildType}";
                    string pingStatus = new string('P', 32);
                    string strNetRoute = new string('N', 32);
                    string strUpTime = $"{(int)MtaServer.Uptime / 10000}";
                    string strHttpPort = Port.ToString();
                    uint extraDataLength = (uint)(strPlayerCount.Length + buildType.Length + buildNumber.Length + pingStatus.Length + strNetRoute.Length + strUpTime.Length + strHttpPort.Length);

                    bw.Write("EYE2".AsSpan());
                    bw.Write((byte)4);
                    bw.Write("mta".AsSpan());
                    bw.Write((byte)(Port.ToString().Length + 1));
                    bw.Write(Port.ToString().AsSpan());
                    bw.Write((byte)(MtaServer.Configuration.ServerName.Length + 1));
                    bw.Write(MtaServer.Configuration.ServerName.AsSpan());
                    bw.Write((byte)(MtaServer.GameType.Length + 1));
                    bw.Write(MtaServer.GameType.AsSpan());
                    bw.Write((byte)(MtaServer.MapName.Length + 7 + 1 + extraDataLength));
                    bw.Write(MtaServer.MapName.AsSpan());
                    bw.Write((byte)0);
                    bw.Write(strPlayerCount.AsSpan());  // client double checks this field in clientside against fake players count function:
                                                            // "CCore::GetSingleton().GetNetwork()->UpdatePingStatus(*strPingStatus, info.players);" 
                    bw.Write((byte)0);
                    bw.Write(buildType.AsSpan());
                    bw.Write((byte)0);
                    bw.Write(buildNumber.AsSpan());
                    bw.Write((byte)0);
                    bw.Write(pingStatus.AsSpan());
                    bw.Write((byte)0);
                    bw.Write(strNetRoute.AsSpan());
                    bw.Write((byte)0);
                    bw.Write(strUpTime.AsSpan());
                    bw.Write((byte)0);
                    bw.Write(strHttpPort.AsSpan());
                    bw.Write((byte)(aseVersion.Length + 1));
                    bw.Write(aseVersion.AsSpan());
                    bw.Write((byte)(MtaServer.HasPassword?1:0)); // password
                    bw.Write((byte)1); // serial verification
                    bw.Write((byte)playersCount); // joined players
                    bw.Write((byte)MtaServer.Configuration.MaxPlayers); // max players

                    int bytesLeft = (1350 - (int)bw.BaseStream.Position);
                    int playersLeftNum = playerNames.Count + 1;
                    foreach (string name in playerNames)
                    {
                        if (bytesLeft - name.Length + 2 > 0)
                        {
                            bw.Write((byte)(name.Length + 1));
                            bw.Write(name.AsSpan());
                            bytesLeft -= name.Length + 2;
                            playersLeftNum--;
                        }
                        else
                        {
                            string playersLeft = $"And {playersLeftNum} more";
                            bw.Write((byte)(playersLeft.Length + 1));
                            bw.Write(playersLeft.AsSpan());
                            break;
                        }
                    }
                    bw.Flush();
                    return stream.ToArray();
                }
            }
        }
        private void OnUdpData(IAsyncResult result)
        {
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint source = new IPEndPoint(0, 0);
            byte[] message = socket.EndReceive(result, ref source);
            byte[] data = new byte[0];
            switch (message[0])
            {
                //case "s": // ASE protocol query
                //    data = QueryLight();
                //    break;
                //case "b":  // Our own lighter query for ingame browser
                //    data = QueryLight();
                //    break;
                case 114: // Our own lighter query for ingame browser - Release version only
                    data = QueryLightCached();
                    break;
                //case "x": // Our own lighter query for xfire updates
                //    data = QueryLight();
                //    break;
                //case "v": // MTA Version (For further possibilities to quick ping, in case we do multiply master servers)
                //    data = QueryLight();
                //    break;
                default:
                    throw new NotImplementedException(message[0].ToString());
            }
            socket.Send(data, data.Length, source);
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
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
