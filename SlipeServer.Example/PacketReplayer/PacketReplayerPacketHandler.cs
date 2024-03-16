using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.PacketHandling.Handlers;
using System;

namespace SlipeServer.Example.PacketReplayer;

public class KeySyncReplayerPacketHandler : IPacketHandler<KeySyncPacket>
{
    private readonly PacketReplayerService packetReplayerService;

    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_KEYSYNC;

    public KeySyncReplayerPacketHandler(PacketReplayerService packetReplayerService)
    {
        this.packetReplayerService = packetReplayerService;
    }

    public void HandlePacket(IClient client, KeySyncPacket packet)
    {
        if (!this.packetReplayerService.ShouldPlayerCaptureKeySyncPackets(client.Player))
            return;

        Directory.CreateDirectory("packetlog/keysync");
        File.WriteAllText($"packetlog/keysync/{(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds}.packet", Newtonsoft.Json.JsonConvert.SerializeObject(packet));
    }
}

public class PureSyncReplayerPacketHandler : IPacketHandler<PlayerPureSyncPacket>
{
    private readonly PacketReplayerService packetReplayerService;

    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_PURESYNC;

    public PureSyncReplayerPacketHandler(PacketReplayerService packetReplayerService)
    {
        this.packetReplayerService = packetReplayerService;
    }

    public void HandlePacket(IClient client, PlayerPureSyncPacket packet)
    {
        if (!this.packetReplayerService.ShouldPlayerCapturePureSyncPackets(client.Player))
            return;

        Directory.CreateDirectory("packetlog/puresync");
        File.WriteAllText($"packetlog/puresync/{(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds}.packet", Newtonsoft.Json.JsonConvert.SerializeObject(packet));
    }
}
