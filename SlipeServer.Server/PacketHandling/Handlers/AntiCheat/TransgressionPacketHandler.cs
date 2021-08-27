using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Transgression;
using SlipeServer.Packets.Enums;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.AntiCheat
{
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

        public void HandlePacket(Client client, TransgressionPacket packet)
        {
            var acRule = (Net.Wrappers.Enums.AntiCheat)packet.Level;
            if (!this.configuration.AntiCheat.DisabledAntiCheat.Contains(acRule))
            {
                client.Player.Kick(packet.Message);
                this.logger.LogWarning($"{client.Player} has trigger anticheat detection for {acRule} {packet.Message}");
            }
        }
    }
}
