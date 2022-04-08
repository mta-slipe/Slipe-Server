using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.PacketHandling.Handlers.Player;

public class PlayerModInfoPacketHandler : IPacketHandler<PlayerModInfoPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_MODINFO;

    public void HandlePacket(Client client, PlayerModInfoPacket packet)
    {
        client.Player.TriggerPlayerModInfo(packet.InfoType, packet.ModInfoItems);
    }
}
