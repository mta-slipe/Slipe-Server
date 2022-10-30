using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.PacketHandling.Handlers.Satchel;

public class PlayerResourceStartedPacketHandler : IPacketHandler<PlayerResourceStartedPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_RESOURCE_START;

    public PlayerResourceStartedPacketHandler()
    {
    }

    public void HandlePacket(IClient client, PlayerResourceStartedPacket packet)
    {
        client.Player.TriggerResourceStarted(packet.NetId);
    }
}
