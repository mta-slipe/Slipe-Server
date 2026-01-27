using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.Services;
using System;
using System.Linq;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for sending light sync information to players that are far away from a to-be-synced player
/// </summary>
public class LightSyncBehaviour
{
    private readonly IElementCollection elementCollection;
    private readonly ISyncHandlerMiddleware<LightSyncBehaviour?> middleware;

    public LightSyncBehaviour(
        IElementCollection elementCollection,
        ISyncHandlerMiddleware<LightSyncBehaviour?> middleware,
        ITimerService timerService,
        Configuration configuration)
    {
        this.elementCollection = elementCollection;
        this.middleware = middleware;

        timerService.CreateTimer(SendLightSyncs, TimeSpan.FromMilliseconds(configuration.SyncIntervals.LightSync));
    }

    private void SendLightSyncs()
    {
        foreach (var player in this.elementCollection.GetByType<Player>(ElementType.Player))
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
