using SlipeServer.Server.PacketHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlipeServer.Server.ServerOptions
{
    public class ServerBuilder
    {
        private readonly List<ServerBuildStep> buildSteps;
        public Configuration Configuration { get; init; }
        public ServerBuilder(Configuration configuration)
        {
            this.buildSteps = new();
            this.Configuration = configuration;
        }

        public void AddBuildStep(Action<MtaServer> step, ServerBuildStepPriority priority = ServerBuildStepPriority.Default)
        {
            this.buildSteps.Add(new ServerBuildStep(step, priority));
        }

        public void AddQueueHandler<T>(params object[] parameters) where T : IQueueHandler
        {
            AddBuildStep(server => server.RegisterPacketQueueHandler<T>(parameters));
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

        public void AddNetWrapper(
            string? directory = null,
            string dllPath = "net.dll",
            string? host = null,
            ushort? port = null,
            AntiCheatConfiguration? antiCheatConfiguration = null)
        {
            AddBuildStep(server => server.AddNetWrapper(
                directory ?? Directory.GetCurrentDirectory(),
                dllPath,
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
    }
}
