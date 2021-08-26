using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server
{
    public static class ServerCollectionExtensions
    {
        public static void AddServerDefaults(this ServiceCollection services, Configuration configuration)
        {
            services.AddSingleton<ISyncHandlerMiddleware<ProjectileSyncPacket>, RangeSyncHandlerMiddleware<ProjectileSyncPacket>>(
                x => new RangeSyncHandlerMiddleware<ProjectileSyncPacket>(x.GetRequiredService<IElementRepository>(), configuration.ExplosionSyncDistance)
            );
            services.AddSingleton<ISyncHandlerMiddleware<DetonateSatchelsPacket>, RangeSyncHandlerMiddleware<DetonateSatchelsPacket>>(
                x => new RangeSyncHandlerMiddleware<DetonateSatchelsPacket>(x.GetRequiredService<IElementRepository>(), configuration.ExplosionSyncDistance, false)
            );
            services.AddSingleton<ISyncHandlerMiddleware<DestroySatchelsPacket>, RangeSyncHandlerMiddleware<DestroySatchelsPacket>>(
                x => new RangeSyncHandlerMiddleware<DestroySatchelsPacket>(x.GetRequiredService<IElementRepository>(), configuration.ExplosionSyncDistance, false)
            );

            services.AddSingleton<ISyncHandlerMiddleware<PlayerPureSyncPacket>, SubscriptionSyncHandlerMiddleware<PlayerPureSyncPacket>>();
            services.AddSingleton<ISyncHandlerMiddleware<KeySyncPacket>, SubscriptionSyncHandlerMiddleware<KeySyncPacket>>();

            services.AddSingleton<ISyncHandlerMiddleware<LightSyncBehaviour>, MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>>(
                x => new MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>(x.GetRequiredService<IElementRepository>(), configuration.LightSyncRange)
            );

        }
    }
}
