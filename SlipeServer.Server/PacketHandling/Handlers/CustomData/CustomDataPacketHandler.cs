using SlipeServer.Packets.Definitions.CustomElementData;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.PacketHandling.Handlers.CustomData;
public class CustomDataPacketHandler(
    IElementCollection elementCollection
    ) : IPacketHandler<CustomDataPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_CUSTOM_DATA;

    public void HandlePacket(IClient client, CustomDataPacket packet)
    {
        var player = client.Player;
        var element = elementCollection.Get(packet.ElementId);
        element?.RunAsSync(() =>
        {
            element.SetData(packet.Key, packet.Value, DataSyncType.Local);
        });
    }
}
