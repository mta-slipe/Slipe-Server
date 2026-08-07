using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Transgression;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.AntiCheat;

public class TransgressionPacketHandler(Configuration configuration, ILogger<TransgressionPacketHandler> logger) : IPacketHandler<TransgressionPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_TRANSGRESSION;

    public void HandlePacket(IClient client, TransgressionPacket packet)
    {
        var acRule = (Net.Wrappers.Enums.AntiCheat)packet.Level;
        if (!configuration.AntiCheat.DisabledAntiCheat.Contains(acRule))
        {
            client.Player.Kick(packet.Message);
            logger.LogWarning("{Player} has trigger anticheat detection for {Rule} {Message}", client.Player.Name, acRule, packet.Message);
        }
    }
}
