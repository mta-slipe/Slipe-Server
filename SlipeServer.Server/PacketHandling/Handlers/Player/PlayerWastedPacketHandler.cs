using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.PacketHandling.Handlers.Player;

public class PlayerWastedPacketHandler : IPacketHandler<PlayerWastedPacket>
{
    private readonly IElementCollection elementCollection;

    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_WASTED;

    public PlayerWastedPacketHandler(
        IElementCollection elementCollection
    )
    {
        this.elementCollection = elementCollection;
    }

    public void HandlePacket(IClient client, PlayerWastedPacket packet)
    {
        var damager = this.elementCollection.Get(packet.KillerId);
        client.Player.Kill(
            damager, (WeaponType)packet.WeaponType, (BodyPart)packet.BodyPart,
            packet.AnimationGroup, packet.AnimationId
        );
    }
}
