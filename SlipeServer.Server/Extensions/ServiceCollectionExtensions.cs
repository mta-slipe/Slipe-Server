using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ServerBuilders;
using System;

namespace SlipeServer.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddMtaServer(this ServiceCollection serviceCollection, Action<ServerBuilder> builderAction, Configuration configuration,
        Func<uint, INetWrapper, IClient>? clientCreationMethod = null,
        ServerBuilderDefaultServices exceptServices = ServerBuilderDefaultServices.None,
        ServerBuilderDefaultMiddleware exceptMiddleware = ServerBuilderDefaultMiddleware.None)
    {
        serviceCollection.AddDefaultMtaServices(configuration, exceptServices, exceptMiddleware);
        serviceCollection.AddSingleton(x => new MtaServer(builderAction, clientCreationMethod, serviceCollection, configuration).Init());
    }

    public static void AddMtaServer<TPlayer>(this ServiceCollection serviceCollection, Action<ServerBuilder> builderAction, Configuration configuration,
        ServerBuilderDefaultServices exceptServices = ServerBuilderDefaultServices.None,
        ServerBuilderDefaultMiddleware exceptMiddleware = ServerBuilderDefaultMiddleware.None) where TPlayer: Player
    {
        serviceCollection.AddDefaultMtaServices(configuration, exceptServices, exceptMiddleware);
        serviceCollection.AddSingleton(x => MtaServer.CreateWithDiSupport<TPlayer>(builderAction, serviceCollection, configuration).Init());
    }

    public static void AddDefaultMtaServices(
        this ServiceCollection services,
        Configuration configuration,
        ServerBuilderDefaultServices exceptServices = ServerBuilderDefaultServices.None,
        ServerBuilderDefaultMiddleware exceptMiddleware = ServerBuilderDefaultMiddleware.None)
    {
        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.ProjectileSyncPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<ProjectileSyncPacket>, RangeSyncHandlerMiddleware<ProjectileSyncPacket>>(
                x => new RangeSyncHandlerMiddleware<ProjectileSyncPacket>(x.GetRequiredService<IElementCollection>(), configuration.ExplosionSyncDistance)
            );
        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.DetonateSatchelsPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<DetonateSatchelsPacket>, RangeSyncHandlerMiddleware<DetonateSatchelsPacket>>(
                x => new RangeSyncHandlerMiddleware<DetonateSatchelsPacket>(x.GetRequiredService<IElementCollection>(), configuration.ExplosionSyncDistance, false)
            );
        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.DestroySatchelsPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<DestroySatchelsPacket>, RangeSyncHandlerMiddleware<DestroySatchelsPacket>>(
                x => new RangeSyncHandlerMiddleware<DestroySatchelsPacket>(x.GetRequiredService<IElementCollection>(), configuration.ExplosionSyncDistance, false)
            );
        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.ExplosionPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<ExplosionPacket>, RangeSyncHandlerMiddleware<ExplosionPacket>>(
                x => new RangeSyncHandlerMiddleware<ExplosionPacket>(x.GetRequiredService<IElementCollection>(), configuration.ExplosionSyncDistance, false)
            );

        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.PlayerPureSyncPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<PlayerPureSyncPacket>, RangeSyncHandlerMiddleware<PlayerPureSyncPacket>>(
                x => new RangeSyncHandlerMiddleware<PlayerPureSyncPacket>(x.GetRequiredService<IElementCollection>(), configuration.LightSyncRange));

        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.KeySyncPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<KeySyncPacket>, RangeSyncHandlerMiddleware<KeySyncPacket>>(
                x => new RangeSyncHandlerMiddleware<KeySyncPacket>(x.GetRequiredService<IElementCollection>(), configuration.LightSyncRange));

        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.LightSyncBehaviourMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<LightSyncBehaviour>, MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>>(
                x => new MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>(x.GetRequiredService<IElementCollection>(), configuration.LightSyncRange));
    }
}
