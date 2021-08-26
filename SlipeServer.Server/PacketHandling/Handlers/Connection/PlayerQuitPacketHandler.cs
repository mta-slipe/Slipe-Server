using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Connection
{
    public class PlayerQuitPacketHandler : IPacketHandler<PlayerQuitPacket>
    {
        private readonly IElementRepository elementRepository;

        public PacketId PacketId => PacketId.PACKET_ID_PLAYER_QUIT;

        public PlayerQuitPacketHandler(IElementRepository elementRepository)
        {
            this.elementRepository = elementRepository;
        }

        public void HandlePacket(Client client, PlayerQuitPacket packet)
        {
            var returnPacket = PlayerPacketFactory.CreateQuitPacket(client.Player, QuitReason.Quit);
            returnPacket.SendTo(this.elementRepository.GetByType<Elements.Player>(ElementType.Player).Except(new Elements.Player[] { client.Player }));

            client.Player.Destroy();
        }
    }
}
