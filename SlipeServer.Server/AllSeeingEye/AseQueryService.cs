using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.ElementCollections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.AllSeeingEye;

/// <summary>
/// Service responsible for generating the response to the ASE queries
/// </summary>
public class AseQueryService : IAseQueryService
{
    private readonly MtaServer mtaServer;
    private readonly Configuration configuration;
    private readonly IElementCollection elementCollection;
    private readonly AseVersion aseVersion;
    private readonly Dictionary<string, string> rules;
    public bool ShowPlayers { get; set; } = true;

    public AseQueryService(MtaServer mtaServer, Configuration configuration, IElementCollection elementCollection)
    {
        this.mtaServer = mtaServer;
        this.configuration = configuration;
        this.elementCollection = elementCollection;

        this.aseVersion = AseVersion.v1_6;

        this.rules = new Dictionary<string, string>();
    }

    public void SetRule(string key, string value)
    {
        this.rules[key] = value;
    }

    public bool RemoveRule(string key) => this.rules.Remove(key);

    public string? GetRule(string key)
    {
        this.rules.TryGetValue(key, out string? value);
        return value;
    }

    private IEnumerable<Player> GetPlayers()
    {
        if (this.ShowPlayers)
            return this.elementCollection.GetByType<Player>(ElementType.Player);
        return Enumerable.Empty<Player>();
    }


    public byte[] QueryFull(ushort port)
    {
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter bw = new BinaryWriter(stream);
        IEnumerable<Player> players = GetPlayers();

        string aseVersion = GetVersion(this.aseVersion);

        bw.Write("EYE1".AsSpan());
        bw.WriteWithLength("mta");
        bw.WriteWithLength(port);
        bw.WriteWithLength(this.configuration.ServerName);
        bw.WriteWithLength(this.mtaServer.GameType);
        bw.WriteWithLength(this.mtaServer.MapName);
        bw.WriteWithLength(aseVersion);
        bw.WriteWithLength(this.mtaServer.HasPassword);
        bw.WriteWithLength(players.Count().ToString());
        bw.WriteWithLength(this.configuration.MaxPlayerCount.ToString());
        foreach (var item in this.rules)
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
            bw.WriteWithLength((int)player.Client.Ping);
            bw.Write((byte)1); // time, skip
        }

        bw.Flush();
        return stream.ToArray();
    }

    public byte[] QueryXFireLight()
    {
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter bw = new BinaryWriter(stream);
        int playerCount = GetPlayers().Count();
        string strPlayerCount = playerCount + "/" + this.configuration.MaxPlayerCount;

        bw.Write("EYE3".AsSpan());
        bw.WriteWithLength("mta");
        bw.WriteWithLength(this.configuration.ServerName);
        bw.WriteWithLength(this.mtaServer.GameType);

        bw.Write((byte)(this.mtaServer.MapName.Length + strPlayerCount.Length + 2));
        bw.Write(this.mtaServer.MapName.AsSpan());
        bw.Write((byte)0);
        bw.Write(strPlayerCount.AsSpan());  // client double checks this field in clientside against fake players count function:
                                            // "CCore::GetSingleton().GetNetwork()->UpdatePingStatus(*strPingStatus, info.players);" 
        bw.WriteWithLength(GetVersion(this.aseVersion));
        bw.Write((byte)(this.mtaServer.HasPassword ? 1 : 0));
        bw.Write((byte)Math.Min(playerCount, 255));
        bw.Write((byte)Math.Min(this.configuration.MaxPlayerCount, (ushort)255));

        bw.Flush();
        return stream.ToArray();
    }

    public byte[] QueryLight(ushort port, VersionType version = VersionType.Release)
    {
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter bw = new BinaryWriter(stream);
        List<string> playerNames = this.elementCollection.GetByType<Player>(ElementType.Player)
            .Where(x => x.Client is not FakeClient)
            .Select(o => o.Name.StripColorCode())
            .ToList();

        var aseVersion = GetVersion(version == VersionType.Release ? this.aseVersion : AseVersion.v1_6n);
        var playerCount = playerNames.Count;

        var strPlayerCount = $"{playerCount} / {this.configuration.MaxPlayerCount}";
        var buildType = $"{(byte)version} ";
        var buildNumber = $"0";
        var pingStatus = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes((ushort)0x728D))
            .Concat(BitConverter.GetBytes((ushort)playerCount))
            .Concat(BitConverter.GetBytes((ushort)0xFFFF))
            .ToArray();
        var strNetRoute = new string('N', 32);
        var strUpTime = $"{(int)this.mtaServer.Uptime.Ticks / TimeSpan.TicksPerSecond}";
        var strHttpPort = this.configuration.HttpPort.ToString();
        uint extraDataLength = (uint)(strPlayerCount.Length + buildType.Length + buildNumber.Length + pingStatus.Length + strNetRoute.Length + strUpTime.Length + strHttpPort.Length) + 7;

        bw.Write("EYE2".AsSpan());
        bw.WriteWithLength("mta");
        bw.WriteWithLength(port);
        bw.WriteWithLength(this.configuration.ServerName);
        bw.WriteWithLength(this.mtaServer.GameType);

        bw.Write((byte)(this.mtaServer.MapName.Length + 1 + extraDataLength));
        bw.Write(this.mtaServer.MapName.AsSpan());
        bw.Write((byte)0);

        bw.Write(strPlayerCount.AsSpan());
        bw.Write((byte)0);

        bw.Write(buildType.AsSpan());
        bw.Write((byte)0);

        bw.Write(buildNumber.AsSpan());
        bw.Write((byte)0);

        bw.Write(pingStatus);
        bw.Write((byte)0);

        bw.Write(strNetRoute.AsSpan());
        bw.Write((byte)0);

        bw.Write(strUpTime.AsSpan());
        bw.Write((byte)0);

        bw.Write(strHttpPort.AsSpan());
        bw.WriteWithLength(aseVersion);
        bw.Write((byte)(this.mtaServer.HasPassword ? 1 : 0));
        bw.Write((byte)0); // serial verification
        bw.Write((byte)Math.Min(playerCount, 255));
        bw.Write((byte)Math.Min(this.configuration.MaxPlayerCount, (ushort)255));

        int bytesLeft = (1340 - (int)bw.BaseStream.Position);
        int playersLeftNum = playerNames.Count + 1;
        foreach (string name in playerNames)
        {
            if (bytesLeft - name.Length + 2 > 0)
            {
                bw.WriteWithLength(name);
                bytesLeft -= name.Length + 2;
                playersLeftNum--;
            } else
            {
                string playersLeft = $"And {playersLeftNum} more";
                bw.WriteWithLength(playersLeft);
                break;
            }
        }
        bw.Flush();
        return stream.ToArray();
    }

    public string GetVersion(AseVersion version = AseVersion.v1_6)
    {
        return version switch
        {
            AseVersion.v1_6 => "1.6",
            AseVersion.v1_6n => "1.6n",
            _ => throw new NotImplementedException(this.aseVersion.ToString()),
        };
    }
}
