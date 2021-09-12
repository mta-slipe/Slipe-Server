using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.PacketHandling.Handlers.Player
{
    public class PlayerNetworkStatusPacketHandler : IPacketHandler<PlayerNetworkStatusPacket>
    {
        public PacketId PacketId => PacketId.PACKET_ID_PLAYER_NETWORK_STATUS;

        public void HandlePacket(Client client, PlayerNetworkStatusPacket packet)
        {
            client.Player.TriggerNetworkStatus(packet.Type, packet.Ticks);
        }
    }
}
