using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.PacketHandling.Handlers.Player.Sync;

public class CameraSyncPacketHandler(
    IElementCollection elementCollection
    ) : IPacketHandler<CameraSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_CAMERA_SYNC;

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
                player.Camera.Target = elementCollection.Get(packet.TargetId);
            }
        });
    }
}
