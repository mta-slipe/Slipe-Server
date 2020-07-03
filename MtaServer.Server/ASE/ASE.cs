using MtaServer.Server.Elements;
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

namespace MtaServer.Server.ASE
{
    public enum AseVersion
    {
        v1_5, // release build
        v1_5n, // debug build
    }

    public enum BuildType
    {
        Custom = 0x01,
        Experimental = 0x03,
        Unstable = 0x05,
        Untested = 0x07,
        Release = 0x09
    }

    [Flags]
    public enum PlayerFlags
    {
        Nick = 0x01,
        Team = 0x02,
        Skin = 0x04,
        Score = 0x08,
        Ping = 0x16,
        Time = 0x32,
    }

    internal static class BinaryWriterExtension
    {
        public static void WriteWithLength(this BinaryWriter bw, string str)
        {
            bw.Write((byte)(str.Length + 1));
            bw.Write(str.AsSpan());
        }

        public static void WriteWithLength(this BinaryWriter bw, int number)
        {
            bw.Write((byte)(number.ToString().Length + 1));
            bw.Write(number.ToString().AsSpan());
        }

        public static void WriteWithLength(this BinaryWriter bw, bool boolean)
        {
            bw.Write((byte)2);
            bw.Write(boolean ? 1 : 0);
        }
    }

    public class ASE
    {
        private int CacheTime { get; } = 10 * 1000;
        internal AseVersion AseVersion { get; }
        internal BuildType BuildType { get; }
        private Cache<byte[]> LightCache { get; }
        private Cache<byte[]> XFireCache { get; }
        private Cache<byte[]> FullCache { get; }
        private Dictionary<string, string> Rules { get; } = new Dictionary<string, string>();

        private readonly IElementRepository elementRepository;
        private readonly Configuration configuration;
        private readonly MtaServer mtaServer;

        public void SetRule(string key, string value)
        {
            this.Rules[key] = value;
        }

        public bool RemoveRule(string key) => this.Rules.Remove(key);

        public string? GetRule(string key)
        {
            if (Rules.TryGetValue(key, out string value))
                return value;
            return null;
        }

        public ASE(MtaServer mtaServer, Configuration configuration, IElementRepository elementRepository)
        {
            this.mtaServer = mtaServer;
            this.elementRepository = elementRepository;
            this.configuration = configuration;

            AseVersion = AseVersion.v1_5;
            BuildType = BuildType.Release;
            StartListening((ushort)(configuration.Port + 123));

            LightCache = new Cache<byte[]>(QueryLight, CacheTime);
            XFireCache = new Cache<byte[]>(QueryXFireLight, CacheTime);
            FullCache = new Cache<byte[]>(QueryFull, CacheTime);
        }

        public string GetVersion()
        {
            string aseVersion;
            switch (AseVersion)
            {
                case AseVersion.v1_5:
                    aseVersion = "1.5";
                    break;
                case AseVersion.v1_5n:
                    aseVersion = "1.5n";
                    break;
                default:
                    throw new NotImplementedException(AseVersion.ToString());
            }
            return aseVersion;
        }

        private byte[] QueryFull()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    IEnumerable<Player> players = elementRepository.GetByType<Player>(ElementType.Player);

                    string aseVersion = GetVersion();
                    int playersCount = players.Count();
                    string strPlayerCount = playersCount.ToString();
                    string strMaxPlayers = configuration.MaxPlayers.ToString();

                    bw.Write("EYE1".AsSpan());
                    bw.WriteWithLength("mta");
                    bw.WriteWithLength(configuration.Port);
                    bw.WriteWithLength(configuration.ServerName);
                    bw.WriteWithLength(mtaServer.MapName);
                    bw.WriteWithLength(aseVersion);
                    bw.WriteWithLength(mtaServer.HasPassword);
                    bw.WriteWithLength(strPlayerCount);
                    bw.WriteWithLength(strMaxPlayers);
                    foreach (var item in Rules)
                    {
                        bw.WriteWithLength(item.Key);
                        bw.WriteWithLength(item.Value);
                    }
                    bw.Write((byte)1);

                    byte flags = 0;
                    flags |= (byte)PlayerFlags.Nick;
                    flags |= (byte)PlayerFlags.Team;
                    flags |= (byte)PlayerFlags.Skin;
                    flags |= (byte)PlayerFlags.Score;
                    flags |= (byte)PlayerFlags.Ping;
                    flags |= (byte)PlayerFlags.Time;

                    foreach(Player player in players)
                    {
                        bw.Write(flags);
                        bw.WriteWithLength(player.NameNoColor);
                        bw.Write((byte)1); // team, skip
                        bw.Write((byte)1); // skin, skip
                        bw.WriteWithLength(player.Score);
                        bw.WriteWithLength(1); // ping, skip right now
                        bw.Write((byte)1); // time, skip
                    }

                    bw.Flush();
                    return stream.ToArray();
                }
            }
        }

        private byte[] QueryXFireLight()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    int playersCount = elementRepository.GetByType<Player>(ElementType.Player).Count();
                    string strPlayerCount = playersCount + "/" + configuration.MaxPlayers;

                    bw.Write("EYE3".AsSpan());
                    bw.WriteWithLength("mta");
                    bw.WriteWithLength(configuration.ServerName);
                    bw.WriteWithLength(mtaServer.GameType);

                    bw.Write((byte)(mtaServer.MapName.Length + strPlayerCount.Length + 2));
                    bw.Write(mtaServer.MapName.AsSpan());
                    bw.Write((byte)0);
                    bw.Write(strPlayerCount.AsSpan());  // client double checks this field in clientside against fake players count function:
                                                        // "CCore::GetSingleton().GetNetwork()->UpdatePingStatus(*strPingStatus, info.players);" 
                    bw.WriteWithLength(GetVersion());
                    bw.Write((byte)(mtaServer.HasPassword ? 1 : 0)); // password
                    bw.Write((byte)playersCount); // joined players
                    bw.Write((byte)configuration.MaxPlayers); // max players

                    bw.Flush();
                    return stream.ToArray();
                }
            }
        }


        private byte[] QueryLight()
        {

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    List<string> playerNames = elementRepository.GetByType<Player>(ElementType.Player).Select(o => o.NameNoColor).ToList();

                    string aseVersion = GetVersion();
                    int playersCount = playerNames.Count();
                    string strPlayerCount = playersCount + "/" + configuration.MaxPlayers;
                    string buildType = $"{(byte)(VersionType.Release)} ";
                    string buildNumber = $"{(byte)BuildType}";
                    string pingStatus = new string('P', 32);
                    string strNetRoute = new string('N', 32);
                    string strUpTime = $"{(int)mtaServer.Uptime / 10000}";
                    string strHttpPort = configuration.Port.ToString();
                    uint extraDataLength = (uint)(strPlayerCount.Length + buildType.Length + buildNumber.Length + pingStatus.Length + strNetRoute.Length + strUpTime.Length + strHttpPort.Length);

                    bw.Write("EYE2".AsSpan());
                    bw.WriteWithLength("mta");
                    bw.WriteWithLength(configuration.Port);
                    bw.WriteWithLength(configuration.ServerName);
                    bw.WriteWithLength(mtaServer.GameType);

                    bw.Write((byte)(mtaServer.MapName.Length + 7 + 1 + extraDataLength));
                    bw.Write(mtaServer.MapName.AsSpan());
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
                    bw.WriteWithLength(aseVersion);
                    bw.Write((byte)(mtaServer.HasPassword ? 1 : 0)); // password
                    bw.Write((byte)1); // serial verification
                    bw.Write((byte)playersCount); // joined players
                    bw.Write((byte)configuration.MaxPlayers); // max players

                    int bytesLeft = (1350 - (int)bw.BaseStream.Position);
                    int playersLeftNum = playerNames.Count + 1;
                    foreach (string name in playerNames)
                    {
                        if (bytesLeft - name.Length + 2 > 0)
                        {
                            bw.WriteWithLength(name);
                            bytesLeft -= name.Length + 2;
                            playersLeftNum--;
                        }
                        else
                        {
                            string playersLeft = $"And {playersLeftNum} more";
                            bw.WriteWithLength(playersLeft);
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
                case 115: // ASE protocol query
                    data = FullCache.Get();
                    break;
                //case 98:  // Our own lighter query for ingame browser
                //    data = QueryLight();
                //    break;
                case 114: // Our own lighter query for ingame browser - Release version only
                    data = LightCache.Get();
                    break;
                case 120: // Our own lighter query for xfire updates
                    data = XFireCache.Get();
                    break;
                //case 118: // MTA Version (For further possibilities to quick ping, in case we do multiply master servers)
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
