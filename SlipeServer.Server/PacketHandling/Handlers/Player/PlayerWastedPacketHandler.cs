using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Enums;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements.Enums;

namespace SlipeServer.Server.PacketHandling.Handlers.Player;

public class PlayerWastedPacketHandler(
    IElementCollection elementCollection
    ) : IPacketHandler<PlayerWastedPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_WASTED;

    public void HandlePacket(IClient client, PlayerWastedPacket packet)
    {
        var damager = elementCollection.Get(packet.KillerId);
        client.Player.Kill(
            damager, (DamageType)packet.WeaponType, (BodyPart)packet.BodyPart,
            packet.AnimationGroup, packet.AnimationId
        );
    }
}
