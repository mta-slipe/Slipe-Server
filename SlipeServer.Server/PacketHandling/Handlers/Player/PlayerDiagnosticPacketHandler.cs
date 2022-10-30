using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.PacketHandling.Handlers.Player;

public class PlayerDiagnosticPacketHandler : IPacketHandler<PlayerDiagnosticPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_DIAGNOSTIC;

    public void HandlePacket(IClient client, PlayerDiagnosticPacket packet)
    {
        if (packet.Level == PlayerDiagnosticPacket.levelSpecialInfo)
        {
            client.Player.TriggerPlayerACInfo(packet.DetectedAC, packet.D3d9Size, packet.D3d9Md5, packet.D3d9Sha256);
        } else
        {
            client.Player.TriggerPlayerDiagnosticInfo(packet.Level, packet.Message);
        }
    }
}
