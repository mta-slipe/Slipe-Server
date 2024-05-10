using SlipeServer.Packets.Definitions.CustomElementData;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Clients;
using System;
using SlipeServer.Server.Concepts;

namespace SlipeServer.Server.PacketHandling.Handlers.CustomData;

[Obsolete("It is highly not recommended to use element data!")]
public class CustomDataPacketHandler : IPacketHandler<CustomDataPacket>
{
    private readonly IElementCollection elementCollection;

    public PacketId PacketId => PacketId.PACKET_ID_CUSTOM_DATA;

    public CustomDataPacketHandler(
        IElementCollection elementCollection
    )
    {
        this.elementCollection = elementCollection;
    }

    public void HandlePacket(IClient client, CustomDataPacket packet)
    {
        var player = client.Player;
        var element = this.elementCollection.Get(packet.ElementId);
        if (element is not ISupportsElementData elementWithCustomData)
            return;

        element?.RunAsSync(() =>
        {
            elementWithCustomData.ElementData.SetData(packet.Key, packet.Value, DataSyncType.Local);
        });
    }
}

public class NoCustomDataPacketHandler : IPacketHandler<CustomDataPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_CUSTOM_DATA;

    public void HandlePacket(IClient client, CustomDataPacket packet)
    {
        var player = client.Player;
        player.Kick(PlayerDisconnectType.KICK);
    }
}
