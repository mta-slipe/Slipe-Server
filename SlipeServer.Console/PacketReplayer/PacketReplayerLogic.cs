using Newtonsoft.Json;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets;
using SlipeServer.Server;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Debugging.PacketRecording;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using SlipeServer.Server.Extensions;
using System.Threading.Tasks;
using SlipeServer.Packets.Definitions.Sync;

namespace SlipeServer.Console.PacketReplayer;

public class PacketReplayerLogic
{
    private readonly MtaServer server;
    private readonly PacketReplayerService packetReplayerService;
    private readonly ChatBox chatBox;
    private readonly Player replayer;

    public PacketReplayerLogic(MtaServer server, CommandService commandService, PacketReplayerService packetReplayerService, ChatBox chatBox)
    {
        this.server = server;
        this.packetReplayerService = packetReplayerService;
        this.chatBox = chatBox;
        this.replayer = new Player();
        var client = new FakeClient(this.replayer)
        {
            Serial = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            IPAddress = System.Net.IPAddress.Parse("127.0.0.1")
        };
        this.replayer.Client = client;
        this.replayer.AssociateWith(server);
        this.replayer.AddWeapon(Server.Enums.WeaponId.Deagle, 500);
        this.replayer.AddWeapon(Server.Enums.WeaponId.Golfclub, 1);

        this.replayer.Name = "Replayer";
        this.replayer.Position = new System.Numerics.Vector3(0, 0.5f, 3);
        this.replayer.Model = 9;
        this.replayer.NametagText = "Fake(Re)Player";
        this.replayer.IsNametagShowing = true;
        this.replayer.NametagColor = System.Drawing.Color.BlueViolet;

        commandService.AddCommand("replaypuresync").Triggered += ReplayPackets;
        commandService.AddCommand("replaykeysync").Triggered += ReplayKeySyncPackets;

        commandService.AddCommand("capturepuresync").Triggered += TogglePureSyncPacketCapture;
        commandService.AddCommand("capturekeysync").Triggered += ToggleKeySyncPacketCapture;

        commandService.AddCommand("mimic").Triggered += HandleMimic;
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

    private void HandleMimic(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        this.chatBox.OutputTo(e.Player, "Replaying started");
        Task.Run(async () =>
        {
            using var _ = new ReplayerPacketRecorder(e.Player, this.server, this.replayer);
            for(int i = 0; i < 15; i++)
            {
                this.chatBox.OutputTo(e.Player, $"{this.replayer.Position}, {e.Player}");
                await Task.Delay(1000);
            }
            this.chatBox.OutputTo(e.Player, "Replaying ended");
        });
    }
}

public class ReplayerPacketRecorder : PacketRecorder
{
    private readonly Player player;
    private readonly MtaServer mtaServer;
    private readonly Player targetPlayer;

    private bool handlingPacket = false;

    public ReplayerPacketRecorder(Player player, MtaServer mtaServer, Player targetPlayer) : base(player, mtaServer)
    {
        this.player = player;
        this.mtaServer = mtaServer;
        this.targetPlayer = targetPlayer;
    }

    protected override void HandlePacketSent(Packet packet, PacketDirection packetDirection)
    {
        if (packetDirection != PacketDirection.Incoming)
            return;

        if (packet is KeySyncPacket keySyncPacket)
            keySyncPacket.PlayerId = this.targetPlayer.Id;
        
        if (packet is PlayerPureSyncPacket pureSyncPacket)
            pureSyncPacket.PlayerId = this.targetPlayer.Id;

        packet.SendTo(this.player);
    }
}

