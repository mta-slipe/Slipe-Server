using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;
using System.Linq;
using System.Timers;

namespace SlipeServer.Server.Behaviour;

public class LightSyncBehaviour
{
    private readonly IElementCollection elementRepository;
    private readonly ISyncHandlerMiddleware<LightSyncBehaviour?> middleware;

    private readonly Timer timer;

    public LightSyncBehaviour(
        IElementCollection elementRepository,
        ISyncHandlerMiddleware<LightSyncBehaviour?> middleware,
        Configuration configuration)
    {
        this.elementRepository = elementRepository;
        this.middleware = middleware;

        this.timer = new Timer(configuration.SyncIntervals.LightSync)
        {
            AutoReset = true,
        };
        this.timer.Start();
        this.timer.Elapsed += (sender, args) => SendLightSyncs();
    }

    private void SendLightSyncs()
    {
        foreach (var player in this.elementRepository.GetByType<Player>(ElementType.Player))
        {
            var otherPlayers = this.middleware.GetPlayersToSyncTo(player, null);

            if (otherPlayers.Any())
            {
                var lightSyncPacket = new LightSyncPacket(
                    elementId: player.Id,
                    timeContext: player.TimeContext,
                    latency: (ushort)player.Client.Ping,
                    health: player.Health,
                    armor: player.Armor,
                    position: player.Position,
                    vehicleHealth: player.Vehicle?.Health
                );
                lightSyncPacket.SendTo(otherPlayers);
            }
        }
    }
}
