using Microsoft.Extensions.Logging;
using MtaServer.Server.Elements;
using MtaServer.Server.Extensions;
using MtaServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MtaServer.Server.AllSeeingEye
{
    public class AseQueryService : IAseQueryService
    {
        private readonly MtaServer mtaServer;
        private readonly Configuration configuration;
        private readonly IElementRepository elementRepository;
        private readonly AseVersion aseVersion;
        private readonly BuildType buildType;
        private readonly Dictionary<string, string> rules;

        public AseQueryService(MtaServer mtaServer, Configuration configuration, IElementRepository elementRepository)
        {
            this.mtaServer = mtaServer;
            this.configuration = configuration;
            this.elementRepository = elementRepository;

            this.aseVersion = AseVersion.v1_5;
            this.buildType = BuildType.Release;

            this.rules = new Dictionary<string, string>();
        }

        public void SetRule(string key, string value)
        {
            this.rules[key] = value;
        }

        public bool RemoveRule(string key) => this.rules.Remove(key);

        public string? GetRule(string key)
        {

            rules.TryGetValue(key, out string? value);
            return value;
        }


        public byte[] QueryFull()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    IEnumerable<Player> players = elementRepository.GetByType<Player>(ElementType.Player);

                    string aseVersion = GetVersion();

                    bw.Write("EYE1".AsSpan());
                    bw.WriteWithLength("mta");
                    bw.WriteWithLength(configuration.Port);
                    bw.WriteWithLength(configuration.ServerName);
                    bw.WriteWithLength(mtaServer.GameType);
                    bw.WriteWithLength(mtaServer.MapName);
                    bw.WriteWithLength(aseVersion);
                    bw.WriteWithLength(mtaServer.HasPassword);
                    bw.WriteWithLength(players.Count().ToString());
                    bw.WriteWithLength(configuration.MaxPlayerCount.ToString());
                    foreach (var item in rules)
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

                    foreach (Player player in players)
                    {
                        bw.Write(flags);
                        bw.WriteWithLength(player.Name.StripColorCode());
                        bw.Write((byte)1); // team, skip
                        bw.Write((byte)1); // skin, skip
                        bw.WriteWithLength(1); // score
                        bw.WriteWithLength(1); // ping, skip right now
                        bw.Write((byte)1); // time, skip
                    }

                    bw.Flush();
                    return stream.ToArray();
                }
            }
        }

        public byte[] QueryXFireLight()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    int playerCount = elementRepository.GetByType<Player>(ElementType.Player).Count();
                    string strPlayerCount = playerCount + "/" + configuration.MaxPlayerCount;

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
                    bw.Write((byte)(mtaServer.HasPassword ? 1 : 0));
                    bw.Write((byte)Math.Min(playerCount, 255));
                    bw.Write((byte)Math.Min(configuration.MaxPlayerCount, (ushort)255));

                    bw.Flush();
                    return stream.ToArray();
                }
            }
        }


        public byte[] QueryLight()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    List<string> playerNames = elementRepository.GetByType<Player>(ElementType.Player)
                        .Select(o => o.Name.StripColorCode())
                        .ToList();

                    string aseVersion = GetVersion();
                    int playerCount = playerNames.Count();
                    string strPlayerCount = playerCount + "/" + configuration.MaxPlayerCount;
                    string buildType = $"{(byte)(VersionType.Release)} ";
                    string buildNumber = $"{(byte)this.buildType}";
                    string pingStatus = new string('P', 32);
                    string strNetRoute = new string('N', 32);
                    string strUpTime = $"{(int)mtaServer.Uptime / 10000}";
                    string strHttpPort = configuration.HttpPort.ToString();
                    uint extraDataLength = (uint)(strPlayerCount.Length + buildType.Length + buildNumber.Length + pingStatus.Length + strNetRoute.Length + strUpTime.Length + strHttpPort.Length) + 7;

                    bw.Write("EYE2".AsSpan());
                    bw.WriteWithLength("mta");
                    bw.WriteWithLength(configuration.Port);
                    bw.WriteWithLength(configuration.ServerName);
                    bw.WriteWithLength(mtaServer.GameType);

                    bw.Write((byte)(mtaServer.MapName.Length + 1 + extraDataLength));
                    bw.Write(mtaServer.MapName.AsSpan());

                    bw.Write((byte)0);
                    bw.Write(strPlayerCount.AsSpan());
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
                    bw.Write((byte)(mtaServer.HasPassword ? 1 : 0));
                    bw.Write((byte)1); // serial verification
                    bw.Write((byte)Math.Min(playerCount, 255));
                    bw.Write((byte)Math.Min(configuration.MaxPlayerCount, (ushort)255));

                    int bytesLeft = (1340 - (int)bw.BaseStream.Position);
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

        public string GetVersion()
        {
            return this.aseVersion switch
            {
                AseVersion.v1_5 => "1.5",
                AseVersion.v1_5n => "1.5n",
                _ => throw new NotImplementedException(this.aseVersion.ToString()),
            };
        }
    }
}
