using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.PacketHandling.Handlers.Player.Sync;

public class CameraSyncPacketHandler : IPacketHandler<CameraSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_CAMERA_SYNC;

    private readonly IElementRepository elementRepository;

    public CameraSyncPacketHandler(
        IElementRepository elementRepository
    )
    {
        this.elementRepository = elementRepository;
    }

    public void HandlePacket(Client client, CameraSyncPacket packet)
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
