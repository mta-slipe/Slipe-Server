using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.PacketHandling.Handlers.Player.Sync;

public class CameraSyncPacketHandler : IPacketHandler<CameraSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_CAMERA_SYNC;

    private readonly IElementCollection elementRepository;

    public CameraSyncPacketHandler(
        IElementCollection elementRepository
    )
    {
        this.elementRepository = elementRepository;
    }

    public void HandlePacket(IClient client, CameraSyncPacket packet)
    {
        var player = client.Player;
        player.RunAsSync(() =>
        {
            if (packet.IsFixed)
            {
                player.Camera.Position = packet.Position;
                player.Camera.LookAt = packet.LookAt;
            } else
            {
                player.Camera.Target = this.elementRepository.Get(packet.TargetId);
            }
        });
    }
}
