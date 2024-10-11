using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Packets.Definitions.CustomElementData;
using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Definitions.Transgression;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Packets.Rpc;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Bans;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements.IdGeneration;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.PacketHandling.Handlers.AntiCheat;
using SlipeServer.Server.PacketHandling.Handlers.BulletSync;
using SlipeServer.Server.PacketHandling.Handlers.Command;
using SlipeServer.Server.PacketHandling.Handlers.Connection;
using SlipeServer.Server.PacketHandling.Handlers.CustomData;
using SlipeServer.Server.PacketHandling.Handlers.Explosions;
using SlipeServer.Server.PacketHandling.Handlers.Lua;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.PacketHandling.Handlers.Player.Sync;
using SlipeServer.Server.PacketHandling.Handlers.Player;
using SlipeServer.Server.PacketHandling.Handlers.Projectile;
using SlipeServer.Server.PacketHandling.Handlers.Rpc;
using SlipeServer.Server.PacketHandling.Handlers.Satchel;
using SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;
using SlipeServer.Server.PacketHandling.Handlers.Vehicle;
using SlipeServer.Server.PacketHandling.Handlers.Voice;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.ServerBuilders;
using SlipeServer.Server.Services;
using SlipeServer.Packets;
using SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.Behaviour;

namespace SlipeServer.Server;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDefaultMtaServerServices(this IServiceCollection services)
    {
        services.AddSingleton<IElementCollection, RTreeCompoundElementCollection>();
        services.AddSingleton<IResourceProvider, FileSystemResourceProvider>();
        services.AddSingleton<IElementIdGenerator, CollectionBasedElementIdGenerator>();
        services.AddSingleton<IAseQueryService, AseQueryService>();
        services.AddSingleton(typeof(ISyncHandlerMiddleware<>), typeof(BasicSyncHandlerMiddleware<>));

        services.AddSingleton<GameWorld>();
        services.AddSingleton<ChatBox>();
        services.AddSingleton<ClientConsole>();
        services.AddSingleton<DebugLog>();
        services.AddSingleton<FromLuaValueMapper>();
        services.AddSingleton<LuaValueMapper>();
        services.AddSingleton<LuaEventService>();
        services.AddSingleton<LatentPacketService>();
        services.AddSingleton<ExplosionService>();
        services.AddSingleton<FireService>();
        services.AddSingleton<TextItemService>();
        services.AddSingleton<WeaponConfigurationService>();
        services.AddSingleton<CommandService>();
        services.AddSingleton<BanService>();
        services.AddSingleton<ITimerService, TimerService>();
        services.TryAddSingleton<IBanRepository, JsonFileBanRepository>();

        return services;
    }

    public static void AddDefaultBehaviours(
        this MtaServer mtaServer,
        ServerBuilderDefaultBehaviours except = ServerBuilderDefaultBehaviours.None)
    {
        if ((except & ServerBuilderDefaultBehaviours.AseBehaviour) == 0)
            mtaServer.InstantiatePersistent<AseBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.LocalServerAnnouncementBehaviour) == 0)
            mtaServer.InstantiatePersistent<LocalServerAnnouncementBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour) == 0)
            mtaServer.InstantiatePersistent<MasterServerAnnouncementBehaviour>("http://master.mtasa.com/ase/add.php");

        if ((except & ServerBuilderDefaultBehaviours.VelocityBehaviour) == 0)
            mtaServer.InstantiatePersistent<VelocityBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.DefaultChatBehaviour) == 0)
            mtaServer.InstantiatePersistent<DefaultChatBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.NicknameChangeBehaviour) == 0)
            mtaServer.InstantiatePersistent<NicknameChangeBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.CollisionShapeBehaviour) == 0)
            mtaServer.InstantiatePersistent<CollisionShapeBehaviour>();

        if ((except & ServerBuilderDefaultBehaviours.PlayerJoinElementBehaviour) == 0)
            mtaServer.InstantiatePersistent<PlayerJoinElementBehaviour>();

        if ((except & ServerBuilderDefaultBehaviours.VoiceBehaviour) == 0)
            mtaServer.InstantiatePersistent<VoiceBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.LightSyncBehaviour) == 0)
            mtaServer.InstantiatePersistent<LightSyncBehaviour>();

        if ((except & ServerBuilderDefaultBehaviours.PickupBehaviour) == 0)
            mtaServer.InstantiatePersistent<PickupBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.MapInfoBehaviour) == 0)
            mtaServer.InstantiatePersistent<MapInfoBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.PedSyncBehaviour) == 0)
            mtaServer.InstantiatePersistent<PedSyncBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.UnoccupiedVehicleSyncBehaviour) == 0)
            mtaServer.InstantiatePersistent<UnoccupiedVehicleSyncBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.CustomDataBehaviour) == 0)
            mtaServer.InstantiatePersistent<CustomDataBehaviour>();
    }
    public static void AddDefaultPacketHandlers(
        this MtaServer mtaServer,
        ServerBuilderDefaultPacketHandlers except = ServerBuilderDefaultPacketHandlers.None)
    {
        if ((except & ServerBuilderDefaultPacketHandlers.JoinedGamePacketHandler) == 0)
            mtaServer.RegisterPacketHandler<JoinedGamePacketHandler, JoinedGamePacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.JoinDataPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<JoinDataPacketHandler, PlayerJoinDataPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerQuitPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PlayerQuitPacketHandler, PlayerQuitPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerTimeoutPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PlayerTimeoutPacketHandler, PlayerTimeoutPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.PlayerPureSyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PlayerPureSyncPacketHandler, PlayerPureSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.KeySyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<KeySyncPacketHandler, KeySyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.CameraSyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<CameraSyncPacketHandler, CameraSyncPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.WeaponBulletSyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<WeaponBulletSyncPacketHandler, WeaponBulletSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerBulletSyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PlayerBulletSyncPacketHandler, PlayerBulletSyncPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.ProjectileSyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<ProjectileSyncPacketHandler, ProjectileSyncPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.ExplosionPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<ExplosionPacketHandler, ExplosionPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.CommandPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<CommandPacketHandler, CommandPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.DetonateSatchelsPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<DetonateSatchelsPacketHandler, DetonateSatchelsPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.DestroySatchelsPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<DestroySatchelsPacketHandler, DestroySatchelsPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.RpcPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<RpcPacketHandler, RpcPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.LuaEventPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<LuaEventPacketHandler, LuaEventPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.PlayerAcInfoPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PlayerAcInfoPacketHandler, PlayerACInfoPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerDiagnosticPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PlayerDiagnosticPacketHandler, PlayerDiagnosticPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerModInfoPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PlayerModInfoPacketHandler, PlayerModInfoPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerNetworkStatusPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PlayerNetworkStatusPacketHandler, PlayerNetworkStatusPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerScreenshotPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PlayerScreenshotPacketHandler, PlayerScreenshotPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerWastedPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PlayerWastedPacketHandler, PlayerWastedPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.VehicleInOutPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<VehicleInOutPacketHandler, VehicleInOutPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.VehiclePureSyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<VehiclePureSyncPacketHandler, VehiclePureSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.VehicleDamageSyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<VehicleDamageSyncPacketHandler, VehicleDamageSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.UnoccupiedVehicleSyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<UnoccupiedVehicleSyncPacketHandler, UnoccupiedVehicleSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.VehiclePushSyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<VehiclePushSyncPacketHandler, VehiclePushSyncPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.VoiceDataPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<VoiceDataPacketHandler, VoiceDataPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.VoiceEndPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<VoiceEndPacketHandler, VoiceEndPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.TransgressionPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<TransgressionPacketHandler, TransgressionPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.PedSyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PedSyncPacketHandler, PedSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PedTaskPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PedTaskPacketHandler, PedTaskPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PedWastedPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PedWastedPacketHandler, PedWastedPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.LatentLuaEventPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<LatentLuaEventPacketHandler, LatentLuaEventPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.CustomDataPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<CustomDataPacketHandler, CustomDataPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.VehicleTrailerSyncPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<VehicleTrailerSyncPacketHandler, VehicleTrailerSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerResourceStartedPacketHandler) == 0)
            mtaServer.RegisterPacketHandler<PlayerResourceStartedPacketHandler, PlayerResourceStartedPacket>();
    }

    public static IServiceCollection AddDefaultMiddlewares(
        this IServiceCollection services,
        ServerBuilderDefaultMiddleware exceptMiddleware = ServerBuilderDefaultMiddleware.None)
    {
        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.ProjectileSyncPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<ProjectileSyncPacket>, RangeSyncHandlerMiddleware<ProjectileSyncPacket>>(
                x => new RangeSyncHandlerMiddleware<ProjectileSyncPacket>(
                    x.GetRequiredService<IElementCollection>(),
                    x.GetRequiredService<Configuration>().ExplosionSyncDistance)
            );
        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.DetonateSatchelsPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<DetonateSatchelsPacket>, RangeSyncHandlerMiddleware<DetonateSatchelsPacket>>(
                x => new RangeSyncHandlerMiddleware<DetonateSatchelsPacket>(
                    x.GetRequiredService<IElementCollection>(),
                    x.GetRequiredService<Configuration>().ExplosionSyncDistance,
                    false)
            );
        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.DestroySatchelsPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<DestroySatchelsPacket>, RangeSyncHandlerMiddleware<DestroySatchelsPacket>>(
                x => new RangeSyncHandlerMiddleware<DestroySatchelsPacket>(
                    x.GetRequiredService<IElementCollection>(),
                    x.GetRequiredService<Configuration>().ExplosionSyncDistance,
                    false)
            );
        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.ExplosionPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<ExplosionPacket>, RangeSyncHandlerMiddleware<ExplosionPacket>>(
                x => new RangeSyncHandlerMiddleware<ExplosionPacket>(
                    x.GetRequiredService<IElementCollection>(),
                    x.GetRequiredService<Configuration>().ExplosionSyncDistance,
                    false)
            );

        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.PlayerPureSyncPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<PlayerPureSyncPacket>, RangeSyncHandlerMiddleware<PlayerPureSyncPacket>>(
                x => new RangeSyncHandlerMiddleware<PlayerPureSyncPacket>(
                    x.GetRequiredService<IElementCollection>(),
                    x.GetRequiredService<Configuration>().LightSyncRange));

        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.KeySyncPacketMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<KeySyncPacket>, RangeSyncHandlerMiddleware<KeySyncPacket>>(
                x => new RangeSyncHandlerMiddleware<KeySyncPacket>(
                    x.GetRequiredService<IElementCollection>(),
                    x.GetRequiredService<Configuration>().LightSyncRange));

        if ((exceptMiddleware & ServerBuilderDefaultMiddleware.LightSyncBehaviourMiddleware) == 0)
            services.AddSingleton<ISyncHandlerMiddleware<LightSyncBehaviour>, MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>>(
                x => new MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>(
                    x.GetRequiredService<IElementCollection>(),
                    x.GetRequiredService<Configuration>().LightSyncRange));

        return services;
    }
}
