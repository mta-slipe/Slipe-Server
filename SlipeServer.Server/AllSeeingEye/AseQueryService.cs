using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SlipeServer.Server.AllSeeingEye;

/// <summary>
/// Generates ASE query responses.
/// The port supplied to QueryFull and QueryLight must be the game port,
/// not the ASE listener port.
/// </summary>
public class AseQueryService(
    IMtaServer mtaServer,
    Configuration configuration,
    IElementCollection elementCollection) : IAseQueryService
{
    private const int MaxFieldBytes = 254;
    private const int MaxLightMapFieldBytes = 250;
    private const int LightPacketBudget = 1340;

    private readonly AseVersion aseVersion = AseVersion.v1_6;
    private readonly Dictionary<string, string> rules = [];
    private readonly object rulesLock = new();

    public bool ShowPlayers { get; set; } = true;

    public void SetRule(string key, string value)
    {
        lock (this.rulesLock)
        {
            this.rules[key] = value;
        }
    }

    public bool RemoveRule(string key)
    {
        lock (this.rulesLock)
        {
            return this.rules.Remove(key);
        }
    }

    public string? GetRule(string key)
    {
        lock (this.rulesLock)
        {
            this.rules.TryGetValue(key, out string? value);
            return value;
        }
    }

    private KeyValuePair<string, string>[] GetRules()
    {
        lock (this.rulesLock)
        {
            return this.rules.ToArray();
        }
    }

    private Player[] GetPlayers()
    {
        if (!this.ShowPlayers)
            return [];

        return elementCollection
            .GetByType<Player>(ElementType.Player)
            .Where(player =>
                player.Client is not FakeClient &&
                player.Client.ConnectionState == ClientConnectionState.Joined)
            .ToArray();
    }

    private static string Number(long value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private static string GetPlayerName(Player player)
    {
        string stripped = player.Name.StripColorCode();

        return string.IsNullOrEmpty(stripped)
            ? player.Name
            : stripped;
    }

    private static byte[] EncodeText(
        string value,
        int maxBytes = MaxFieldBytes)
    {
        if (maxBytes < 0 || maxBytes > MaxFieldBytes)
            throw new ArgumentOutOfRangeException(nameof(maxBytes));

        // Embedded NUL bytes would interfere with compound ASE fields.
        value = value.Replace("\0", string.Empty);

        if (maxBytes == 0 || value.Length == 0)
            return Array.Empty<byte>();

        if (Encoding.UTF8.GetByteCount(value) <= maxBytes)
            return Encoding.UTF8.GetBytes(value);

        // Truncate without splitting a UTF-8 character.
        byte[] buffer = new byte[maxBytes];

        Encoding.UTF8.GetEncoder().Convert(
            value.AsSpan(),
            buffer.AsSpan(),
            flush: true,
            out _,
            out int bytesUsed,
            out _);

        return buffer.AsSpan(0, bytesUsed).ToArray();
    }

    private static void WriteField(BinaryWriter writer, byte[] bytes)
    {
        if (bytes.Length > MaxFieldBytes)
            throw new ArgumentOutOfRangeException(nameof(bytes));

        // The prefix includes its own byte.
        // Ordinary fields do not have a terminating NUL.
        writer.Write(checked((byte)(bytes.Length + 1)));
        writer.Write(bytes);
    }

    private static void WriteField(BinaryWriter writer, string value) =>
        WriteField(writer, EncodeText(value));

    private static void WriteAscii(BinaryWriter writer, string value) =>
        writer.Write(Encoding.ASCII.GetBytes(value));

    private static void WriteNullTerminated(
        BinaryWriter writer,
        byte[] bytes)
    {
        writer.Write(bytes);
        writer.Write((byte)0);
    }

    private static void WriteNullTerminated(
        BinaryWriter writer,
        string value)
    {
        WriteAscii(writer, value);
        writer.Write((byte)0);
    }

    public byte[] QueryFull(ushort port)
    {
        Player[] players = GetPlayers();
        KeyValuePair<string, string>[] ruleSnapshot = GetRules();

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        WriteAscii(writer, "EYE1");
        WriteField(writer, "mta");
        WriteField(writer, Number(port));
        WriteField(writer, configuration.ServerName);
        WriteField(writer, mtaServer.GameType);
        WriteField(writer, mtaServer.MapName);
        WriteField(writer, GetVersion(this.aseVersion));
        WriteField(writer, mtaServer.HasPassword ? "1" : "0");
        WriteField(writer, Number(players.Length));
        WriteField(writer, Number(configuration.MaxPlayerCount));

        foreach (var rule in ruleSnapshot)
        {
            // An empty key is the rules terminator in this protocol.
            if (string.IsNullOrEmpty(rule.Key.Replace("\0", string.Empty)))
                continue;

            WriteField(writer, rule.Key);
            WriteField(writer, rule.Value);
        }

        writer.Write((byte)1);

        // Nick, team, skin, score, ping and time.
        // Explicit mask avoids the incorrect Ping/Time enum values.
        const byte playerFlags = 0x3F;

        foreach (Player player in players)
        {
            writer.Write(playerFlags);
            WriteField(writer, GetPlayerName(player));
            WriteField(writer, string.Empty); // Team unavailable.
            WriteField(writer, string.Empty); // Skin omitted.
            WriteField(writer, string.Empty); // Score unavailable.
            WriteField(writer, Number(player.Client.Ping));
            WriteField(writer, string.Empty); // Time unavailable.
        }

        writer.Flush();
        return stream.ToArray();
    }

    public byte[] QueryXFireLight()
    {
        int playerCount = GetPlayers().Length;

        byte[] countBytes = Encoding.ASCII.GetBytes(
            $"{Number(playerCount)}/{Number(configuration.MaxPlayerCount)}");

        byte[] mapBytes = EncodeText(
            mtaServer.MapName,
            MaxFieldBytes - countBytes.Length - 1);

        using var mapStream = new MemoryStream();

        using (var mapWriter = new BinaryWriter(
            mapStream,
            Encoding.UTF8,
            leaveOpen: true))
        {
            WriteNullTerminated(mapWriter, mapBytes);
            mapWriter.Write(countBytes);
        }

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        WriteAscii(writer, "EYE3");
        WriteField(writer, "mta");
        WriteField(writer, configuration.ServerName);
        WriteField(writer, mtaServer.GameType);
        WriteField(writer, mapStream.ToArray());
        WriteField(writer, GetVersion(this.aseVersion));

        writer.Write((byte)(mtaServer.HasPassword ? 1 : 0));
        writer.Write((byte)Math.Min(playerCount, 255));
        writer.Write((byte)Math.Min(
            (int)configuration.MaxPlayerCount,
            255));

        writer.Flush();
        return stream.ToArray();
    }

    public byte[] QueryLight(
        ushort port,
        VersionType version = VersionType.Release)
    {
        Player[] players = GetPlayers();

        byte[][] playerNames = players
            .Select(GetPlayerName)
            .Select(name => EncodeText(name))
            .ToArray();

        int playerCount = players.Length;

        string responseVersion = GetVersion(
            version == VersionType.Release
                ? this.aseVersion
                : AseVersion.v1_6n);

        string strPlayerCount =
            $"{Number(playerCount)}/{Number(configuration.MaxPlayerCount)}";

        string buildType = Number((byte)version);

        // TODO: Use metadata matching the actual network module/build.
        string buildNumber = "0";

        var netWrapper = mtaServer.GetNetWrapper(port);

        byte[] pingStatus = netWrapper.GetAsePingStatus();
        byte[] netRouteBytes = netWrapper.GetAseNetRoute();

        string strUpTime = Number(
            mtaServer.Uptime.Ticks / TimeSpan.TicksPerSecond);

        string strHttpPort = Number(configuration.HttpPort);

        using var extraStream = new MemoryStream();

        using (var extraWriter = new BinaryWriter(
            extraStream,
            Encoding.UTF8,
            leaveOpen: true))
        {
            WriteNullTerminated(extraWriter, strPlayerCount);
            WriteNullTerminated(extraWriter, buildType);
            WriteNullTerminated(extraWriter, buildNumber);
            WriteNullTerminated(extraWriter, pingStatus);
            WriteNullTerminated(extraWriter, netRouteBytes);
            WriteNullTerminated(extraWriter, strUpTime);

            // The last component has no terminating NUL.
            WriteAscii(extraWriter, strHttpPort);
        }

        byte[] extraBytes = extraStream.ToArray();

        int maxMapBytes = MaxLightMapFieldBytes - 1 - extraBytes.Length;

        if (maxMapBytes < 0)
        {
            throw new InvalidOperationException(
                "ASE metadata exceeds the compound map field budget.");
        }

        byte[] mapBytes = EncodeText(mtaServer.MapName, maxMapBytes);

        using var mapStream = new MemoryStream();

        using (var mapWriter = new BinaryWriter(
            mapStream,
            Encoding.UTF8,
            leaveOpen: true))
        {
            WriteNullTerminated(mapWriter, mapBytes);
            mapWriter.Write(extraBytes);
        }

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        WriteAscii(writer, "EYE2");
        WriteField(writer, "mta");
        WriteField(writer, Number(port));
        WriteField(writer, configuration.ServerName);
        WriteField(writer, mtaServer.GameType);
        WriteField(writer, mapStream.ToArray());
        WriteField(writer, responseVersion);

        writer.Write((byte)(mtaServer.HasPassword ? 1 : 0));
        writer.Write((byte)0); // Serial verification currently unavailable.
        writer.Write((byte)Math.Min(playerCount, 255));
        writer.Write((byte)Math.Min(
            (int)configuration.MaxPlayerCount,
            255));

        int bytesLeft = LightPacketBudget - checked((int)stream.Position);
        int playersLeft = playerNames.Length;

        foreach (byte[] nameBytes in playerNames)
        {
            // Preserve one field per player, as the MTA implementation does.
            // Empty fields replace names that exceed the packet budget.
            bytesLeft -= nameBytes.Length + 1;

            bool includeName = bytesLeft >= playersLeft;
            playersLeft--;

            WriteField(
                writer,
                includeName ? nameBytes : Array.Empty<byte>());
        }

        writer.Flush();
        return stream.ToArray();
    }

    public string GetVersion(AseVersion version = AseVersion.v1_6)
    {
        return version switch
        {
            AseVersion.v1_6 => "1.6",
            AseVersion.v1_6n => "1.6n",
            _ => throw new ArgumentOutOfRangeException(
                nameof(version),
                version,
                "Unsupported ASE version.")
        };
    }
}
