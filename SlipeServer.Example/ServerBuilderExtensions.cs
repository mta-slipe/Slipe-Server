using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Example.Logic;
using SlipeServer.Example.PacketReplayer;
using SlipeServer.Example.Proxy;
using SlipeServer.Example.Services;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ServerBuilders;
using SlipeServer.Example.AdditionalResources.Parachute;
using SlipeServer.Physics.Extensions;
using SlipeServer.LuaControllers;
using SlipeServer.Lua;

namespace SlipeServer.Example;

public static class ServerBuilderExtensions
{
    public static ServerBuilder ConfigureExampleServer(this ServerBuilder builder, Server.Configuration configuration)
    {
        builder.UseConfiguration(configuration);

#if DEBUG
        builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
#else
        builder.AddDefaults();
#endif

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<ISyncHandlerMiddleware<PlayerPureSyncPacket>, SubscriptionSyncHandlerMiddleware<PlayerPureSyncPacket>>();
            services.AddSingleton<ISyncHandlerMiddleware<KeySyncPacket>, SubscriptionSyncHandlerMiddleware<KeySyncPacket>>();

            services.AddScoped<TestService>();
            services.AddSingleton<PacketReplayerService>();
            services.AddScoped<SampleScopedService>();
        });
        builder.AddLua();
        builder.AddPhysics();
        builder.AddParachuteResource();
        builder.AddLuaControllers();

        builder.AddLogic<ServerTestLogic>();
        builder.AddLogic<LuaTestLogic>();
        builder.AddLogic<PhysicsTestLogic>();
        builder.AddLogic<ElementPoolingTestLogic>();
        builder.AddLogic<WarpIntoVehicleLogic>();
        builder.AddLogic<LuaEventTestLogic>();
        builder.AddLogic<ServiceUsageTestLogic>();
        builder.AddLogic<NametagTestLogic>();
        builder.AddLogic<VehicleTestLogic>();
        builder.AddLogic<ClothingTestLogic>();
        builder.AddLogic<PedTestLogic>();
        builder.AddLogic<ProxyService>();
        builder.AddScopedLogic<ScopedTestLogic1>();
        builder.AddScopedLogic<ScopedTestLogic2>();
        builder.AddLogic(typeof(TestLogic));
        //builder.AddBehaviour<VelocityBehaviour>();
        builder.AddBehaviour<EventLoggingBehaviour>();

        builder.AddPacketHandler<KeySyncReplayerPacketHandler, KeySyncPacket>();
        builder.AddPacketHandler<PureSyncReplayerPacketHandler, PlayerPureSyncPacket>();
        builder.AddLogic<PacketReplayerLogic>();

        return builder;
    }

}
