using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;

namespace SlipeServer.Console.PacketReplayer;

public class PacketReplayerLogic
{
    private readonly PacketReplayerService packetReplayerService;
    private readonly Player replayer;

    public PacketReplayerLogic(MtaServer server, CommandService commandService, PacketReplayerService packetReplayerService)
    {
        this.packetReplayerService = packetReplayerService;

        this.replayer = new Player()
        {
            Name = "Replayer",
            Position = new System.Numerics.Vector3(0, 0.5f, 3),
            Model = 9,
            NametagText = "Fake(Re)Player",
            IsNametagShowing = true,
            NametagColor = System.Drawing.Color.BlueViolet
        };
        var client = new FakeClient(this.replayer)
        {
            Serial = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            IPAddress = System.Net.IPAddress.Parse("127.0.0.1")
        };
        this.replayer.Client = client;
        this.replayer.AssociateWith(server);
        this.replayer.AddWeapon(Server.Enums.WeaponId.Deagle, 500);
        this.replayer.AddWeapon(Server.Enums.WeaponId.Golfclub, 1);

        commandService.AddCommand("replaypuresync").Triggered += ReplayPackets;
        commandService.AddCommand("replaykeysync").Triggered += ReplayKeySyncPackets;

        commandService.AddCommand("capturepuresync").Triggered += TogglePureSyncPacketCapture;
        commandService.AddCommand("capturekeysync").Triggered += ToggleKeySyncPacketCapture; ;
    }

    private void ReplayPackets(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        this.packetReplayerService.ReplayPureSync(this.replayer, e.Player);
    }

    private void ReplayKeySyncPackets(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        this.packetReplayerService.ReplayKeySync(this.replayer, e.Player);
    }

    private void ToggleKeySyncPacketCapture(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        this.packetReplayerService.ToggleKeySyncPacketCapture(e.Player);
    }

    private void TogglePureSyncPacketCapture(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        this.packetReplayerService.TogglePureSyncPacketCapture(e.Player);
    }
}
