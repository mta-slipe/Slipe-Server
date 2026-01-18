using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Services;
using System.Text.Json;

namespace SlipeServer.Console.PacketReplayer;

public class PacketReplayerService
{
    private readonly static JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HashSet<Player> pureSyncCapturingPlayers;
    private readonly HashSet<Player> keySyncCapturingPlayers;
    private readonly ILogger logger;
    private readonly ChatBox chatBox;

    public PacketReplayerService(MtaServer server, ILogger logger, ChatBox chatBox)
    {
        this.logger = logger;
        this.chatBox = chatBox;

        this.pureSyncCapturingPlayers = new();
        this.keySyncCapturingPlayers = new();

        server.ForAny<Player>(RegisterPlayerHandlers);
    }

    private void RegisterPlayerHandlers(Player player)
    {
        player.Disconnected += (s, e) =>
        {
            this.pureSyncCapturingPlayers.Remove(player);
            this.keySyncCapturingPlayers.Remove(player);
        };
    }

    public void TogglePureSyncPacketCapture(Player player)
    {
        if (this.pureSyncCapturingPlayers.Contains(player))
        {
            this.pureSyncCapturingPlayers.Remove(player);
            this.logger.LogInformation("{player} stopped logging pure sync packets", player.Name);
            this.chatBox.OutputTo(player, "Stopped logging pure sync packets");
        } else
        {
            this.pureSyncCapturingPlayers.Add(player);
            this.logger.LogInformation("{player} started logging pure sync packets", player.Name);
            this.chatBox.OutputTo(player, "Started logging pure sync packets");
        }

    }

    public bool ShouldPlayerCapturePureSyncPackets(Player player)
    {
        return this.pureSyncCapturingPlayers.Contains(player);
    }

    public void ToggleKeySyncPacketCapture(Player player)
    {
        if (this.keySyncCapturingPlayers.Contains(player))
        {
            this.keySyncCapturingPlayers.Remove(player);
            this.logger.LogInformation("{player} stopped logging key sync packets", player.Name);
            this.chatBox.OutputTo(player, "Stopped logging key sync packets");
        } else
        {
            this.keySyncCapturingPlayers.Add(player);
            this.logger.LogInformation("{player} started logging key sync packets", player.Name);
            this.chatBox.OutputTo(player, "Started logging key sync packets");
        }

    }

    public bool ShouldPlayerCaptureKeySyncPackets(Player player)
    {
        return this.keySyncCapturingPlayers.Contains(player);
    }

    public async void ReplayPureSync(Player replayer, Player player)
    {
        var packets = System.IO.Directory.GetFiles("packetlog/puresync")
            .OrderBy(x => x)
            .Select(x => System.IO.File.ReadAllText(x))
            .Select(x => JsonSerializer.Deserialize<PlayerPureSyncPacket>(x, jsonOptions))
            .Where(x => x != null);

        foreach (var packet in packets)
        {
            packet!.PlayerId = replayer.Id;
            packet.SendTo(player);
            await Task.Delay(50);
            this.logger.LogInformation("Pure Sync packet sent");
        }
    }

    public async void ReplayKeySync(Player replayer, Player player)
    {
        var packets = System.IO.Directory.GetFiles("packetlog/keysync")
            .OrderBy(x => x)
            .Select(x => System.IO.File.ReadAllText(x))
            .Select(x => JsonSerializer.Deserialize<KeySyncPacket>(x, jsonOptions))
            .Where(x => x != null);

        foreach (var packet in packets)
        {
            packet!.PlayerId = replayer.Id;
            packet.SendTo(player);
            await Task.Delay(50);
            this.logger.LogInformation("Key Sync packet sent");
        }
    }
}
