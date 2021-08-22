using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using System.IO;

namespace SlipeServer.Server.ServerOptions
{
    public static class DefaultServerBuilderExtensions
    {
        public static void AddDefaultQueueHandlers(this ServerBuilder builder)
        {
            builder.AddQueueHandler<ExplosionSyncQueueHandler>(10, 1);
            builder.AddQueueHandler<ConnectionQueueHandler>(10, 1);
            builder.AddQueueHandler<RpcQueueHandler>(10, 1);
            builder.AddQueueHandler<SyncQueueHandler>(QueueHandlerScalingConfig.Aggressive, 10);
            builder.AddQueueHandler<CommandQueueHandler>(10, 1);
            builder.AddQueueHandler<LuaEventQueueHandler>(10, 1);
            builder.AddQueueHandler<PlayerEventQueueHandler>(10, 1);
            builder.AddQueueHandler<VehicleInOutHandler>(10, 1);
            builder.AddQueueHandler<VehicleSyncQueueHandler>(QueueHandlerScalingConfig.Aggressive, 10);
            builder.AddQueueHandler<VoiceHandler>(10, 1);
            builder.AddQueueHandler<SatchelQueueHandler>(10, 1);
        }

        public static void AddDefaultBehaviours(this ServerBuilder builder)
        {
            builder.AddBehaviour<AseBehaviour>();
            builder.AddBehaviour<MasterServerAnnouncementBehaviour>("http://master.mtasa.com/ase/add.php");

            builder.AddBehaviour<EventLoggingBehaviour>();
            builder.AddBehaviour<VelocityBehaviour>();
            builder.AddBehaviour<DefaultChatBehaviour>();
            builder.AddBehaviour<NicknameChangeBehaviour>();
            builder.AddBehaviour<CollisionShapeBehaviour>();

            builder.AddBehaviour<PlayerJoinElementBehaviour>();

            builder.AddBehaviour<ElementPacketBehaviour>();
            builder.AddBehaviour<PedPacketBehaviour>();
            builder.AddBehaviour<PlayerPacketBehaviour>();
            builder.AddBehaviour<VehicleWarpBehaviour>();
            builder.AddBehaviour<VoiceBehaviour>();
            builder.AddBehaviour<LightSyncBehaviour>();
            builder.AddBehaviour<RadarAreaBehaviour>();
        }

        public static void AddDefaults(this ServerBuilder builder)
        {
            builder.AddDefaultQueueHandlers();
            builder.AddDefaultBehaviours();
            builder.AddNetWrapper(
                Directory.GetCurrentDirectory(), 
                "net.dll", 
                builder.Configuration.Host, 
                builder.Configuration.Port, 
                builder.Configuration.AntiCheat);
        }
    }
}
