using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.PacketHandling.Handlers.Player
{
    public class PlayerWastedPacketHandler : IPacketHandler<PlayerWastedPacket>
    {
        private readonly IElementRepository elementRepository;

        public PacketId PacketId => PacketId.PACKET_ID_PLAYER_WASTED;

        public PlayerWastedPacketHandler(
            IElementRepository elementRepository
        )
        {
            this.elementRepository = elementRepository;
        }

        public void HandlePacket(Client client, PlayerWastedPacket packet)
        {
            var damager = this.elementRepository.Get(packet.KillerId);
            client.Player.Kill(
                damager, (WeaponType)packet.WeaponType, (BodyPart)packet.BodyPart,
                packet.AnimationGroup, packet.AnimationId
            );
        }
    }
}
