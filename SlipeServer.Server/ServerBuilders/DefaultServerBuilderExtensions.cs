using Microsoft.Extensions.DependencyInjection;
using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Definitions.Transgression;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Packets.Rpc;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.PacketHandling.Handlers.AntiCheat;
using SlipeServer.Server.PacketHandling.Handlers.BulletSync;
using SlipeServer.Server.PacketHandling.Handlers.Command;
using SlipeServer.Server.PacketHandling.Handlers.Connection;
using SlipeServer.Server.PacketHandling.Handlers.Explosions;
using SlipeServer.Server.PacketHandling.Handlers.Lua;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.PacketHandling.Handlers.Player;
using SlipeServer.Server.PacketHandling.Handlers.Player.Sync;
using SlipeServer.Server.PacketHandling.Handlers.Projectile;
using SlipeServer.Server.PacketHandling.Handlers.Rpc;
using SlipeServer.Server.PacketHandling.Handlers.Satchel;
using SlipeServer.Server.PacketHandling.Handlers.Vehicle;
using SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;
using SlipeServer.Server.PacketHandling.Handlers.Voice;
using SlipeServer.Server.ElementCollections;
using System.IO;
using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using SlipeServer.Server.PacketHandling.Handlers.CustomData;
using SlipeServer.Packets.Definitions.CustomElementData;
using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Resources.Interpreters;

namespace SlipeServer.Server.ServerBuilders;

public static class DefaultServerBuilderExtensions
{
    public static void AddDefaultPacketHandler(
        this ServerBuilder builder,
        ServerBuilderDefaultPacketHandlers except = ServerBuilderDefaultPacketHandlers.None)
    {
        if ((except & ServerBuilderDefaultPacketHandlers.JoinedGamePacketHandler) == 0)
            builder.AddPacketHandler<JoinedGamePacketHandler, JoinedGamePacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.JoinDataPacketHandler) == 0)
            builder.AddPacketHandler<JoinDataPacketHandler, PlayerJoinDataPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerQuitPacketHandler) == 0)
            builder.AddPacketHandler<PlayerQuitPacketHandler, PlayerQuitPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerTimeoutPacketHandler) == 0)
            builder.AddPacketHandler<PlayerTimeoutPacketHandler, PlayerTimeoutPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.PlayerPureSyncPacketHandler) == 0)
            builder.AddPacketHandler<PlayerPureSyncPacketHandler, PlayerPureSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.KeySyncPacketHandler) == 0)
            builder.AddPacketHandler<KeySyncPacketHandler, KeySyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.CameraSyncPacketHandler) == 0)
            builder.AddPacketHandler<CameraSyncPacketHandler, CameraSyncPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.WeaponBulletSyncPacketHandler) == 0)
            builder.AddPacketHandler<WeaponBulletSyncPacketHandler, WeaponBulletSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerBulletSyncPacketHandler) == 0)
            builder.AddPacketHandler<PlayerBulletSyncPacketHandler, PlayerBulletSyncPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.ProjectileSyncPacketHandler) == 0)
            builder.AddPacketHandler<ProjectileSyncPacketHandler, ProjectileSyncPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.ExplosionPacketHandler) == 0)
            builder.AddPacketHandler<ExplosionPacketHandler, ExplosionPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.CommandPacketHandler) == 0)
            builder.AddPacketHandler<CommandPacketHandler, CommandPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.DetonateSatchelsPacketHandler) == 0)
            builder.AddPacketHandler<DetonateSatchelsPacketHandler, DetonateSatchelsPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.DestroySatchelsPacketHandler) == 0)
            builder.AddPacketHandler<DestroySatchelsPacketHandler, DestroySatchelsPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.RpcPacketHandler) == 0)
            builder.AddPacketHandler<RpcPacketHandler, RpcPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.LuaEventPacketHandler) == 0)
            builder.AddPacketHandler<LuaEventPacketHandler, LuaEventPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.PlayerAcInfoPacketHandler) == 0)
            builder.AddPacketHandler<PlayerAcInfoPacketHandler, PlayerACInfoPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerDiagnosticPacketHandler) == 0)
            builder.AddPacketHandler<PlayerDiagnosticPacketHandler, PlayerDiagnosticPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerModInfoPacketHandler) == 0)
            builder.AddPacketHandler<PlayerModInfoPacketHandler, PlayerModInfoPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerNetworkStatusPacketHandler) == 0)
            builder.AddPacketHandler<PlayerNetworkStatusPacketHandler, PlayerNetworkStatusPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerScreenshotPacketHandler) == 0)
            builder.AddPacketHandler<PlayerScreenshotPacketHandler, PlayerScreenshotPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerWastedPacketHandler) == 0)
            builder.AddPacketHandler<PlayerWastedPacketHandler, PlayerWastedPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.VehicleInOutPacketHandler) == 0)
            builder.AddPacketHandler<VehicleInOutPacketHandler, VehicleInOutPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.VehiclePureSyncPacketHandler) == 0)
            builder.AddPacketHandler<VehiclePureSyncPacketHandler, VehiclePureSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.VehicleDamageSyncPacketHandler) == 0)
            builder.AddPacketHandler<VehicleDamageSyncPacketHandler, VehicleDamageSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.UnoccupiedVehicleSyncPacketHandler) == 0)
            builder.AddPacketHandler<UnoccupiedVehicleSyncPacketHandler, UnoccupiedVehicleSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.VehiclePushSyncPacketHandler) == 0)
            builder.AddPacketHandler<VehiclePushSyncPacketHandler, VehiclePushSyncPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.VoiceDataPacketHandler) == 0)
            builder.AddPacketHandler<VoiceDataPacketHandler, VoiceDataPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.VoiceEndPacketHandler) == 0)
            builder.AddPacketHandler<VoiceEndPacketHandler, VoiceEndPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.TransgressionPacketHandler) == 0)
            builder.AddPacketHandler<TransgressionPacketHandler, TransgressionPacket>();

        if ((except & ServerBuilderDefaultPacketHandlers.PedSyncPacketHandler) == 0)
            builder.AddPacketHandler<PedSyncPacketHandler, PedSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PedTaskPacketHandler) == 0)
            builder.AddPacketHandler<PedTaskPacketHandler, PedTaskPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PedWastedPacketHandler) == 0)
            builder.AddPacketHandler<PedWastedPacketHandler, PedWastedPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.LatentLuaEventPacketHandler) == 0)
            builder.AddPacketHandler<LatentLuaEventPacketHandler, LatentLuaEventPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.CustomDataPacketHandler) == 0)
            builder.AddPacketHandler<CustomDataPacketHandler, CustomDataPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.VehicleTrailerSyncPacketHandler) == 0)
            builder.AddPacketHandler<VehicleTrailerSyncPacketHandler, VehicleTrailerSyncPacket>();
        if ((except & ServerBuilderDefaultPacketHandlers.PlayerResourceStartedPacketHandler) == 0)
            builder.AddPacketHandler<PlayerResourceStartedPacketHandler, PlayerResourceStartedPacket>();
    }

    public static void AddDefaultBehaviours(
        this ServerBuilder builder,
        ServerBuilderDefaultBehaviours except = ServerBuilderDefaultBehaviours.None)
    {
        if ((except & ServerBuilderDefaultBehaviours.AseBehaviour) == 0)
            builder.AddBehaviour<AseBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.LocalServerAnnouncementBehaviour) == 0)
            builder.AddBehaviour<LocalServerAnnouncementBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour) == 0)
            builder.AddBehaviour<MasterServerAnnouncementBehaviour>("http://master.mtasa.com/ase/add.php");

        if ((except & ServerBuilderDefaultBehaviours.VelocityBehaviour) == 0)
            builder.AddBehaviour<VelocityBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.DefaultChatBehaviour) == 0)
            builder.AddBehaviour<DefaultChatBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.NicknameChangeBehaviour) == 0)
            builder.AddBehaviour<NicknameChangeBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.CollisionShapeBehaviour) == 0)
            builder.AddBehaviour<CollisionShapeBehaviour>();

        if ((except & ServerBuilderDefaultBehaviours.PlayerJoinElementBehaviour) == 0)
            builder.AddBehaviour<PlayerJoinElementBehaviour>();

        if ((except & ServerBuilderDefaultBehaviours.ElementPacketBehaviour) == 0)
            builder.AddBehaviour<ElementPacketBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.PedPacketBehaviour) == 0)
            builder.AddBehaviour<PedPacketBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.PlayerPacketBehaviour) == 0)
            builder.AddBehaviour<PlayerPacketBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.VehicleWarpBehaviour) == 0)
            builder.AddBehaviour<VehicleWarpBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.VehicleRespawnBehaviour) == 0)
            builder.AddBehaviour<VehicleRespawnBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.VehicleBehaviour) == 0)
            builder.AddBehaviour<VehicleBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.VoiceBehaviour) == 0)
            builder.AddBehaviour<VoiceBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.LightSyncBehaviour) == 0)
            builder.AddBehaviour<LightSyncBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.TeamBehaviour) == 0)
            builder.AddBehaviour<TeamBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.RadarAreaBehaviour) == 0)
            builder.AddBehaviour<RadarAreaBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.BlipBehaviour) == 0)
            builder.AddBehaviour<BlipBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.ObjectPacketBehaviour) == 0)
            builder.AddBehaviour<ObjectPacketBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.PickupBehaviour) == 0)
            builder.AddBehaviour<PickupBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.MarkerBehaviour) == 0)
            builder.AddBehaviour<MarkerBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.MapInfoBehaviour) == 0)
            builder.AddBehaviour<MapInfoBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.PedSyncBehaviour) == 0)
            builder.AddBehaviour<PedSyncBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.UnoccupiedVehicleSyncBehaviour) == 0)
            builder.AddBehaviour<UnoccupiedVehicleSyncBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.CustomDataBehaviour) == 0)
            builder.AddBehaviour<CustomDataBehaviour>();
        if ((except & ServerBuilderDefaultBehaviours.PlayerBehaviour) == 0)
            builder.AddBehaviour<PlayerBehaviour>();
    }

    public static void AddDefaultServices(
        this ServerBuilder builder,
        ServerBuilderDefaultServices exceptServices = ServerBuilderDefaultServices.None,
        ServerBuilderDefaultMiddleware exceptMiddleware = ServerBuilderDefaultMiddleware.None)
    {
        builder.ConfigureServices(services =>
        {
            if ((exceptMiddleware & ServerBuilderDefaultMiddleware.ProjectileSyncPacketMiddleware) == 0)
                services.AddSingleton<ISyncHandlerMiddleware<ProjectileSyncPacket>, RangeSyncHandlerMiddleware<ProjectileSyncPacket>>(
                    x => new RangeSyncHandlerMiddleware<ProjectileSyncPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.ExplosionSyncDistance)
                );
            if ((exceptMiddleware & ServerBuilderDefaultMiddleware.DetonateSatchelsPacketMiddleware) == 0)
                services.AddSingleton<ISyncHandlerMiddleware<DetonateSatchelsPacket>, RangeSyncHandlerMiddleware<DetonateSatchelsPacket>>(
                    x => new RangeSyncHandlerMiddleware<DetonateSatchelsPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.ExplosionSyncDistance, false)
                );
            if ((exceptMiddleware & ServerBuilderDefaultMiddleware.DestroySatchelsPacketMiddleware) == 0)
                services.AddSingleton<ISyncHandlerMiddleware<DestroySatchelsPacket>, RangeSyncHandlerMiddleware<DestroySatchelsPacket>>(
                    x => new RangeSyncHandlerMiddleware<DestroySatchelsPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.ExplosionSyncDistance, false)
                );
            if ((exceptMiddleware & ServerBuilderDefaultMiddleware.ExplosionPacketMiddleware) == 0)
                services.AddSingleton<ISyncHandlerMiddleware<ExplosionPacket>, RangeSyncHandlerMiddleware<ExplosionPacket>>(
                    x => new RangeSyncHandlerMiddleware<ExplosionPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.ExplosionSyncDistance, false)
                );

            if ((exceptMiddleware & ServerBuilderDefaultMiddleware.PlayerPureSyncPacketMiddleware) == 0)
                services.AddSingleton<ISyncHandlerMiddleware<PlayerPureSyncPacket>, RangeSyncHandlerMiddleware<PlayerPureSyncPacket>>(
                    x => new RangeSyncHandlerMiddleware<PlayerPureSyncPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.LightSyncRange));

            if ((exceptMiddleware & ServerBuilderDefaultMiddleware.KeySyncPacketMiddleware) == 0)
                services.AddSingleton<ISyncHandlerMiddleware<KeySyncPacket>, RangeSyncHandlerMiddleware<KeySyncPacket>>(
                    x => new RangeSyncHandlerMiddleware<KeySyncPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.LightSyncRange));

            if ((exceptMiddleware & ServerBuilderDefaultMiddleware.LightSyncBehaviourMiddleware) == 0)
                services.AddSingleton<ISyncHandlerMiddleware<LightSyncBehaviour>, MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>>(
                    x => new MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>(x.GetRequiredService<IElementCollection>(), builder.Configuration.LightSyncRange));


        });
    }
    public static void AddDefaultResourceInterpreters(
        this ServerBuilder builder,
        ServerBuilderDefaultResourceInterpreters except = ServerBuilderDefaultResourceInterpreters.None)
    {
        if (!except.HasFlag(ServerBuilderDefaultResourceInterpreters.Basic))
            builder.AddResourceInterpreter<BasicResourceInterpreter>();

        if (!except.HasFlag(ServerBuilderDefaultResourceInterpreters.MetaXml))
            builder.AddResourceInterpreter<MetaXmlResourceInterpreter>();

        if (!except.HasFlag(ServerBuilderDefaultResourceInterpreters.SlipeLua))
            builder.AddResourceInterpreter<SlipeLuaResourceInterpreter>();
    }

    public static void AddDefaults(
        this ServerBuilder builder,
        ServerBuilderDefaultPacketHandlers exceptPacketHandlers = ServerBuilderDefaultPacketHandlers.None,
        ServerBuilderDefaultBehaviours exceptBehaviours = ServerBuilderDefaultBehaviours.None,
        ServerBuilderDefaultServices exceptServices = ServerBuilderDefaultServices.None,
        ServerBuilderDefaultMiddleware exceptMiddleware = ServerBuilderDefaultMiddleware.None,
        ServerBuilderDefaultResourceInterpreters exceptResourceInterpreters = ServerBuilderDefaultResourceInterpreters.None)
    {
        builder.AddDefaultPacketHandler(exceptPacketHandlers);
        builder.AddDefaultBehaviours(exceptBehaviours);
        builder.AddDefaultServices(exceptServices, exceptMiddleware);
        builder.AddDefaultLuaMappings();

        builder.AddResourceServer<BasicHttpServer>();
        builder.AddDefaultResourceInterpreters(exceptResourceInterpreters);

        builder.AddNetWrapper(
            Directory.GetCurrentDirectory(),
            "net",
            builder.Configuration.Host,
            builder.Configuration.Port,
            builder.Configuration.AntiCheat);
    }
}
