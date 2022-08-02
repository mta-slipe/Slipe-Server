using Microsoft.Extensions.DependencyInjection;
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
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.PacketHandling.Handlers.AntiCheat;
using SlipeServer.Server.PacketHandling.Handlers.BulletSync;
using SlipeServer.Server.PacketHandling.Handlers.Command;
using SlipeServer.Server.PacketHandling.Handlers.Connection;
using SlipeServer.Server.PacketHandling.Handlers.CustomData;
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
using SlipeServer.Server.PacketHandling.QueueHandlers;
using SlipeServer.Server.Resources.Serving;
using System.IO;

namespace SlipeServer.Server.ServerBuilders;

public static class DefaultServerBuilderExtensions
{
    public static void AddDefaultPacketHandler(
        this ServerBuilder builder,
        ServerBuilderDefaultPacketHandlers except = ServerBuilderDefaultPacketHandlers.None)
    {
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.JoinedGamePacketHandler))
            builder.AddPacketHandler<JoinedGamePacketHandler, JoinedGamePacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.JoinDataPacketHandler))
            builder.AddPacketHandler<JoinDataPacketHandler, PlayerJoinDataPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PlayerQuitPacketHandler))
            builder.AddPacketHandler<PlayerQuitPacketHandler, PlayerQuitPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PlayerTimeoutPacketHandler))
            builder.AddPacketHandler<PlayerTimeoutPacketHandler, PlayerTimeoutPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PlayerPureSyncPacketHandler))
            builder.AddPacketHandler<PlayerPureSyncPacketHandler, PlayerPureSyncPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.KeySyncPacketHandler))
            builder.AddPacketHandler<KeySyncPacketHandler, KeySyncPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.CameraSyncPacketHandler))
            builder.AddPacketHandler<CameraSyncPacketHandler, CameraSyncPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.WeaponBulletSyncPacketHandler))
            builder.AddPacketHandler<WeaponBulletSyncPacketHandler, WeaponBulletSyncPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PlayerBulletSyncPacketHandler))
            builder.AddPacketHandler<PlayerBulletSyncPacketHandler, PlayerBulletSyncPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.ProjectileSyncPacketHandler))
            builder.AddPacketHandler<ProjectileSyncPacketHandler, ProjectileSyncPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.ExplosionPacketHandler))
            builder.AddPacketHandler<ExplosionPacketHandler, ExplosionPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.CommandPacketHandler))
            builder.AddPacketHandler<CommandPacketHandler, CommandPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.DetonateSatchelsPacketHandler))
            builder.AddPacketHandler<DetonateSatchelsPacketHandler, DetonateSatchelsPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.DestroySatchelsPacketHandler))
            builder.AddPacketHandler<DestroySatchelsPacketHandler, DestroySatchelsPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.RpcPacketHandler))
            builder.AddPacketHandler<RpcPacketHandler, RpcPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.LuaEventPacketHandler))
            builder.AddPacketHandler<LuaEventPacketHandler, LuaEventPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PlayerAcInfoPacketHandler))
            builder.AddPacketHandler<PlayerAcInfoPacketHandler, PlayerACInfoPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PlayerDiagnosticPacketHandler))
            builder.AddPacketHandler<PlayerDiagnosticPacketHandler, PlayerDiagnosticPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PlayerModInfoPacketHandler))
            builder.AddPacketHandler<PlayerModInfoPacketHandler, PlayerModInfoPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PlayerNetworkStatusPacketHandler))
            builder.AddPacketHandler<PlayerNetworkStatusPacketHandler, PlayerNetworkStatusPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PlayerScreenshotPacketHandler))
            builder.AddPacketHandler<PlayerScreenshotPacketHandler, PlayerScreenshotPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PlayerWastedPacketHandler))
            builder.AddPacketHandler<PlayerWastedPacketHandler, PlayerWastedPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.VehicleInOutPacketHandler))
            builder.AddPacketHandler<VehicleInOutPacketHandler, VehicleInOutPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.VehiclePureSyncPacketHandler))
            builder.AddPacketHandler<VehiclePureSyncPacketHandler, VehiclePureSyncPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.VehicleDamageSyncPacketHandler))
            builder.AddPacketHandler<VehicleDamageSyncPacketHandler, VehicleDamageSyncPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.UnoccupiedVehicleSyncPacketHandler))
            builder.AddPacketHandler<UnoccupiedVehicleSyncPacketHandler, UnoccupiedVehicleSyncPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.VehiclePushSyncPacketHandler))
            builder.AddPacketHandler<VehiclePushSyncPacketHandler, VehiclePushSyncPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.VoiceDataPacketHandler))
            builder.AddPacketHandler<VoiceDataPacketHandler, VoiceDataPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.VoiceEndPacketHandler))
            builder.AddPacketHandler<VoiceEndPacketHandler, VoiceEndPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.TransgressionPacketHandler))
            builder.AddPacketHandler<TransgressionPacketHandler, TransgressionPacket>();

        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PedSyncPacketHandler))
            builder.AddPacketHandler<PedSyncPacketHandler, PedSyncPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PedTaskPacketHandler))
            builder.AddPacketHandler<PedTaskPacketHandler, PedTaskPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PedWastedPacketHandler))
            builder.AddPacketHandler<PedWastedPacketHandler, PedWastedPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.LatentLuaEventPacketHandler))
            builder.AddPacketHandler<LatentLuaEventPacketHandler, LatentLuaEventPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.CustomDataPacketHandler))
            builder.AddPacketHandler<CustomDataPacketHandler, CustomDataPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.VehicleTrailerSyncPacketHandler))
            builder.AddPacketHandler<VehicleTrailerSyncPacketHandler, VehicleTrailerSyncPacket>();
        if (!except.HasFlag(ServerBuilderDefaultPacketHandlers.PlayerResourceStartedPacketHandler))
            builder.AddPacketHandler<PlayerResourceStartedPacketHandler, PlayerResourceStartedPacket>();
    }

    public static void AddDefaultBehaviours(
        this ServerBuilder builder,
        ServerBuilderDefaultBehaviours except = ServerBuilderDefaultBehaviours.None)
    {
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.AseBehaviour))
            builder.AddBehaviour<AseBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.LocalServerAnnouncementBehaviour))
            builder.AddBehaviour<LocalServerAnnouncementBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour))
            builder.AddBehaviour<MasterServerAnnouncementBehaviour>("http://master.mtasa.com/ase/add.php");

        if (!except.HasFlag(ServerBuilderDefaultBehaviours.VelocityBehaviour))
            builder.AddBehaviour<VelocityBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.DefaultChatBehaviour))
            builder.AddBehaviour<DefaultChatBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.NicknameChangeBehaviour))
            builder.AddBehaviour<NicknameChangeBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.CollisionShapeBehaviour))
            builder.AddBehaviour<CollisionShapeBehaviour>();

        if (!except.HasFlag(ServerBuilderDefaultBehaviours.PlayerJoinElementBehaviour))
            builder.AddBehaviour<PlayerJoinElementBehaviour>();

        if (!except.HasFlag(ServerBuilderDefaultBehaviours.ElementPacketBehaviour))
            builder.AddBehaviour<ElementPacketBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.PedPacketBehaviour))
            builder.AddBehaviour<PedPacketBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.PlayerPacketBehaviour))
            builder.AddBehaviour<PlayerPacketBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.VehicleWarpBehaviour))
            builder.AddBehaviour<VehicleWarpBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.VehicleRespawnBehaviour))
            builder.AddBehaviour<VehicleRespawnBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.VehicleBehaviour))
            builder.AddBehaviour<VehicleBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.VoiceBehaviour))
            builder.AddBehaviour<VoiceBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.LightSyncBehaviour))
            builder.AddBehaviour<LightSyncBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.TeamBehaviour))
            builder.AddBehaviour<TeamBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.RadarAreaBehaviour))
            builder.AddBehaviour<RadarAreaBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.BlipBehaviour))
            builder.AddBehaviour<BlipBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.ObjectPacketBehaviour))
            builder.AddBehaviour<ObjectPacketBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.PickupBehaviour))
            builder.AddBehaviour<PickupBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.MarkerBehaviour))
            builder.AddBehaviour<MarkerBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.MapInfoBehaviour))
            builder.AddBehaviour<MapInfoBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.PedSyncBehaviour))
            builder.AddBehaviour<PedSyncBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.UnoccupiedVehicleSyncBehaviour))
            builder.AddBehaviour<UnoccupiedVehicleSyncBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.CustomDataBehaviour))
            builder.AddBehaviour<CustomDataBehaviour>();
        if (!except.HasFlag(ServerBuilderDefaultBehaviours.PlayerBehaviour))
            builder.AddBehaviour<PlayerBehaviour>();
    }

    public static void AddDefaultServices(
        this ServerBuilder builder,
        ServerBuilderDefaultServices exceptServices = ServerBuilderDefaultServices.None,
        ServerBuilderDefaultMiddleware exceptMiddleware = ServerBuilderDefaultMiddleware.None)
    {
        builder.ConfigureServices(services =>
        {
            if (!exceptMiddleware.HasFlag(ServerBuilderDefaultMiddleware.ProjectileSyncPacketMiddleware))
                services.AddSingleton<ISyncHandlerMiddleware<ProjectileSyncPacket>, RangeSyncHandlerMiddleware<ProjectileSyncPacket>>(
                    x => new RangeSyncHandlerMiddleware<ProjectileSyncPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.ExplosionSyncDistance)
                );
            if (!exceptMiddleware.HasFlag(ServerBuilderDefaultMiddleware.DetonateSatchelsPacketMiddleware))
                services.AddSingleton<ISyncHandlerMiddleware<DetonateSatchelsPacket>, RangeSyncHandlerMiddleware<DetonateSatchelsPacket>>(
                    x => new RangeSyncHandlerMiddleware<DetonateSatchelsPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.ExplosionSyncDistance, false)
                );
            if (!exceptMiddleware.HasFlag(ServerBuilderDefaultMiddleware.DestroySatchelsPacketMiddleware))
                services.AddSingleton<ISyncHandlerMiddleware<DestroySatchelsPacket>, RangeSyncHandlerMiddleware<DestroySatchelsPacket>>(
                    x => new RangeSyncHandlerMiddleware<DestroySatchelsPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.ExplosionSyncDistance, false)
                );
            if (!exceptMiddleware.HasFlag(ServerBuilderDefaultMiddleware.ExplosionPacketMiddleware))
                services.AddSingleton<ISyncHandlerMiddleware<ExplosionPacket>, RangeSyncHandlerMiddleware<ExplosionPacket>>(
                    x => new RangeSyncHandlerMiddleware<ExplosionPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.ExplosionSyncDistance, false)
                );

            if (!exceptMiddleware.HasFlag(ServerBuilderDefaultMiddleware.PlayerPureSyncPacketMiddleware))
                services.AddSingleton<ISyncHandlerMiddleware<PlayerPureSyncPacket>, RangeSyncHandlerMiddleware<PlayerPureSyncPacket>>(
                    x => new RangeSyncHandlerMiddleware<PlayerPureSyncPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.LightSyncRange));

            if (!exceptMiddleware.HasFlag(ServerBuilderDefaultMiddleware.KeySyncPacketMiddleware))
                services.AddSingleton<ISyncHandlerMiddleware<KeySyncPacket>, RangeSyncHandlerMiddleware<KeySyncPacket>>(
                    x => new RangeSyncHandlerMiddleware<KeySyncPacket>(x.GetRequiredService<IElementCollection>(), builder.Configuration.LightSyncRange));

            if (!exceptMiddleware.HasFlag(ServerBuilderDefaultMiddleware.LightSyncBehaviourMiddleware))
                services.AddSingleton<ISyncHandlerMiddleware<LightSyncBehaviour>, MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>>(
                    x => new MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>(x.GetRequiredService<IElementCollection>(), builder.Configuration.LightSyncRange));


        });
    }

    public static void AddDefaults(
        this ServerBuilder builder,
        ServerBuilderDefaultPacketHandlers exceptPacketHandlers = ServerBuilderDefaultPacketHandlers.None,
        ServerBuilderDefaultBehaviours exceptBehaviours = ServerBuilderDefaultBehaviours.None,
        ServerBuilderDefaultServices exceptServices = ServerBuilderDefaultServices.None,
        ServerBuilderDefaultMiddleware exceptMiddleware = ServerBuilderDefaultMiddleware.None,
        ServerBuilderDefaultRelayers? exceptRelayers = null)
    {
        builder.AddDefaultPacketHandler(exceptPacketHandlers);
        builder.AddDefaultBehaviours(exceptBehaviours);
        builder.AddDefaultServices(exceptServices, exceptMiddleware);
        builder.AddDefaultRelayers(exceptRelayers ?? ServerBuilderDefaultRelayers.None);
        builder.AddDefaultLuaMappings();

        builder.AddResourceServer<BasicHttpServer>();

        builder.AddNetWrapper(
            Directory.GetCurrentDirectory(),
            "net",
            builder.Configuration.Host,
            builder.Configuration.Port,
            builder.Configuration.AntiCheat);
    }
}
