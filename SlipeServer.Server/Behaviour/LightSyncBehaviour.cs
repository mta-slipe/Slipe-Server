using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware;

namespace SlipeServer.Server.Behaviour
{
    public class LightSyncBehaviour
    {
        private readonly ISyncHandlerMiddleware<object> middleware;

        public LightSyncBehaviour(MtaServer server, ISyncHandlerMiddleware<object> middleware)
        {
            server.PlayerJoined += HandlePlayerJoin;
            this.middleware = middleware;
        }

        private void HandlePlayerJoin(Player player)
        {
            player.PureSynced += HandlePlayerPuresync;
        }

        private void HandlePlayerPuresync(Player sender, System.EventArgs e)
        {
            var lightSyncPacket = new LightSyncPacket()
            {
                ElementId = sender.Id,
                TimeContext = sender.TimeContext,
                Latency = (ushort)sender.Client.Ping,
                Health = sender.Health,
                Armor = sender.Armor,
                Position = sender.Position,
                VehicleHealth = sender.Vehicle?.Health
            };

            var otherPlayers = this.middleware.GetPlayersToSyncTo(sender, new());
            lightSyncPacket.SendTo(otherPlayers);
        }
    }
}
