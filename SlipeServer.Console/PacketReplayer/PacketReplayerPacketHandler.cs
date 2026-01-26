using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.PacketHandling.Handlers;
using System.Text.Json;

namespace SlipeServer.Console.PacketReplayer;

public class KeySyncReplayerPacketHandler(PacketReplayerService packetReplayerService) : IPacketHandler<KeySyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_KEYSYNC;

    public void HandlePacket(IClient client, KeySyncPacket packet)
    {
        if (!packetReplayerService.ShouldPlayerCaptureKeySyncPackets(client.Player))
            return;

        System.IO.Directory.CreateDirectory("packetlog/keysync");
        System.IO.File.WriteAllText($"packetlog/keysync/{(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds}.packet", JsonSerializer.Serialize(packet));
    }
}

public class PureSyncReplayerPacketHandler(PacketReplayerService packetReplayerService) : IPacketHandler<PlayerPureSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_PURESYNC;

    public void HandlePacket(IClient client, PlayerPureSyncPacket packet)
    {
        if (!packetReplayerService.ShouldPlayerCapturePureSyncPackets(client.Player))
            return;

        System.IO.Directory.CreateDirectory("packetlog/puresync");
        System.IO.File.WriteAllText($"packetlog/puresync/{(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds}.packet", JsonSerializer.Serialize(packet));
    }
}
