using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.PacketHandling.Handlers.Player;

public class PlayerAcInfoPacketHandler : IPacketHandler<PlayerACInfoPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_ACINFO;

    public void HandlePacket(IClient client, PlayerACInfoPacket packet)
    {
        client.Player.TriggerPlayerACInfo(packet.DetectedACList, packet.D3d9Size, packet.D3d9MD5, packet.D3d9SHA256);
    }
}
