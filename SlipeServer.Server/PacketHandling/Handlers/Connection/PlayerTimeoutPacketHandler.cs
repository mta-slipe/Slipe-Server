using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Connection;

public class PlayerTimeoutPacketHandler : IPacketHandler<PlayerTimeoutPacket>
{
    private readonly IElementRepository elementRepository;

    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_TIMEOUT;

    public PlayerTimeoutPacketHandler(IElementRepository elementRepository)
    {
        this.elementRepository = elementRepository;
    }

    public void HandlePacket(Client client, PlayerTimeoutPacket packet)
    {
        var returnPacket = PlayerPacketFactory.CreateQuitPacket(client.Player, QuitReason.Timeout);
        returnPacket.SendTo(this.elementRepository.GetByType<Elements.Player>(ElementType.Player).Except(new Elements.Player[] { client.Player }));

        client.Player.Destroy();
    }
}
