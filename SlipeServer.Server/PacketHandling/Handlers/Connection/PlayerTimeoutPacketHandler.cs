using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.ElementCollections;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Connection;

public class PlayerTimeoutPacketHandler : IPacketHandler<PlayerTimeoutPacket>
{
    private readonly IElementCollection elementCollection;

    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_TIMEOUT;

    public PlayerTimeoutPacketHandler(IElementCollection elementCollection)
    {
        this.elementCollection = elementCollection;
    }

    public void HandlePacket(IClient client, PlayerTimeoutPacket packet)
    {
        var returnPacket = PlayerPacketFactory.CreateQuitPacket(client.Player, QuitReason.Timeout);
        returnPacket.SendTo(this.elementCollection.GetByType<Elements.Player>(ElementType.Player).Except(new Elements.Player[] { client.Player }));

        client.Player.Destroy();
    }
}
