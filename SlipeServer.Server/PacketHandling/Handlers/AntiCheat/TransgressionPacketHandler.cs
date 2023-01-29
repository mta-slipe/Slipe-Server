using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Transgression;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.AntiCheat;

public class TransgressionPacketHandler : IPacketHandler<TransgressionPacket>
{
    private readonly Configuration configuration;
    private readonly ILogger logger;

    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_TRANSGRESSION;

    public TransgressionPacketHandler(Configuration configuration, ILogger logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public void HandlePacket(IClient client, TransgressionPacket packet)
    {
        var acRule = (Net.Wrappers.Enums.AntiCheat)packet.Level;
        if (!this.configuration.AntiCheat.DisabledAntiCheat.Contains(acRule))
        {
            client.Player.Kick(packet.Message);
            this.logger.LogWarning($"{client.Player.Name} has trigger anticheat detection for {acRule} {packet.Message}");
        }
    }
}
