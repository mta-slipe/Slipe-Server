using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Packets;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SlipeServer.Server.ServerOptions
{
    public class ServerBuilder
    {
        private readonly List<ServerBuildStep> buildSteps;
        public Configuration Configuration { get; private set; }
        private readonly List<Action<ServiceCollection>> dependecyLoaders;

        public ServerBuilder()
        {
            this.Configuration = new();
            this.buildSteps = new();
            this.dependecyLoaders = new();
        }

        public void UseConfiguration(Configuration configuration)
        {
            this.Configuration = configuration;
        }

        public void AddBuildStep(Action<MtaServer> step, ServerBuildStepPriority priority = ServerBuildStepPriority.Default)
        {
            this.buildSteps.Add(new ServerBuildStep(step, priority));
        }

        public void AddPacketHandler<TPacketQueueHandler, TPacketHandler, TPacket>(params object[] parameters)
            where TPacketQueueHandler : class, IPacketQueueHandler<TPacket>
            where TPacketHandler : IPacketHandler<TPacket>
            where TPacket : Packet, new()
        {
            AddBuildStep(server => server.RegisterPacketHandler<TPacket, TPacketQueueHandler, TPacketHandler>(parameters));
        }

        public void AddPacketHandler<TPacketHandler, TPacket>(params object[] parameters)
            where TPacketHandler : IPacketHandler<TPacket>
            where TPacket : Packet, new()
        {
            AddBuildStep(server => server.RegisterPacketHandler<TPacket, ScalingPacketQueueHandler<TPacket>, TPacketHandler>(parameters));
        }

        public void Instantiate<T>(params object[] parameters)
        {
            AddBuildStep(server => server.Instantiate<T>(parameters));
        }

        public void AddBehaviour<T>(params object[] parameters)
        {
            Instantiate<T>(parameters);
        }

        public void AddLogic<T>(params object[] parameters)
        {
            Instantiate<T>(parameters);
        }

        public void ConfigureServices(Action<ServiceCollection> action)
        {
            this.dependecyLoaders.Add(action);
        }

        public void AddNetWrapper(
            string? directory = null,
            string dllPath = "net",
            string? host = null,
            ushort? port = null,
            AntiCheatConfiguration? antiCheatConfiguration = null)
        {
            AddBuildStep(server => server.AddNetWrapper(
                directory ?? Directory.GetCurrentDirectory(),
                Path.GetFileNameWithoutExtension(dllPath) + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll" : ".so"),
                host ?? this.Configuration.Host,
                port ?? this.Configuration.Port,
                antiCheatConfiguration
            ));
        }

        public void ApplyTo(MtaServer server)
        {
            foreach (var step in this.buildSteps.OrderBy(x => (int)x.Priority))
                step.Step(server);
        }

        public void LoadDependencies(ServiceCollection services)
        {
            foreach (var loader in this.dependecyLoaders)
                loader(services);
        }
    }
}
