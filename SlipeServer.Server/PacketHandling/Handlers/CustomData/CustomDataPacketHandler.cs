using SlipeServer.Packets.Definitions.CustomElementData;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.PacketHandling.Handlers.CustomData;
public class CustomDataPacketHandler : IPacketHandler<CustomDataPacket>
{
    private readonly IElementRepository elementRepository;

    public PacketId PacketId => PacketId.PACKET_ID_CUSTOM_DATA;

    public CustomDataPacketHandler(
        IElementRepository elementRepository
    )
    {
        this.elementRepository = elementRepository;
    }

    public void HandlePacket(IClient client, CustomDataPacket packet)
    {
        var player = client.Player;
        var element = this.elementRepository.Get(packet.ElementId);
        element?.RunAsSync(() =>
        {
            element.SetData(packet.Key, packet.Value, DataSyncType.Local);
        });
    }
}
