using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements.Enums;

namespace SlipeServer.Server.PacketHandling.QueueHandlers;

public class PedWastedPacketHandler(IElementCollection elementCollection) : IPacketHandler<PedWastedPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PED_WASTED;

    public void HandlePacket(IClient client, PedWastedPacket packet)
    {
        if (elementCollection.Get(packet.SourceElementId) is not Ped ped)
            return;

        ped.RunAsSync(() =>
        {
            ped.Kill(elementCollection.Get(packet.KillerId), (DamageType)packet.KillerWeapon, (BodyPart)packet.BodyPart, packet.AnimGroup, packet.AnimId);
        });
    }
}
