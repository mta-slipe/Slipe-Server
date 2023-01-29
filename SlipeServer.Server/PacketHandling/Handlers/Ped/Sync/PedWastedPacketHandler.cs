using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.PacketHandling.QueueHandlers;

public class PedWastedPacketHandler : IPacketHandler<PedWastedPacket>
{
    private readonly IElementCollection elementCollection;

    public PacketId PacketId => PacketId.PACKET_ID_PED_WASTED;

    public PedWastedPacketHandler(IElementCollection elementCollection)
    {
        this.elementCollection = elementCollection;
    }

    public void HandlePacket(IClient client, PedWastedPacket packet)
    {
        if (this.elementCollection.Get(packet.SourceElementId) is not Ped ped)
            return;

        ped.RunAsSync(() =>
        {
            ped.Kill(this.elementCollection.Get(packet.KillerId), (WeaponType)packet.KillerWeapon, (BodyPart)packet.BodyPart, packet.AnimGroup, packet.AnimId);
        });
    }
}
