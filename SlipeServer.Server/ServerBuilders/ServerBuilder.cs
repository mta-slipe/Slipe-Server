using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Packets;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;
using SlipeServer.Server.Resources.Serving;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SlipeServer.Server.ServerBuilders;

public class ServerBuilder
{
    private readonly List<ServerBuildStep> buildSteps;
    private readonly List<PostDependencyBuildStep> postDependencyBuildSteps;
    public Configuration Configuration { get; private set; }
    private readonly List<Action<IServiceCollection>> dependecyLoaders;
    public IServiceCollection? ServiceCollection { get; private set; }
    public IServiceProvider? ServiceProvider { get; private set; }

    public ServerBuilder()
    {
        this.Configuration = new();
        this.buildSteps = new();
        this.postDependencyBuildSteps = new();
        this.dependecyLoaders = new();
    }

    public void UseConfiguration(Configuration configuration)
    {
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(configuration, new ValidationContext(configuration), validationResults, true))
        {
            string invalidProperties = string.Join("\r\n\t", validationResults.Select(r => r.ErrorMessage));
            throw new Exception($"An error has occurred while parsing configuration parameters:\r\n {invalidProperties}");
        }
        this.Configuration = configuration;
    }

    public void UseServiceCollection(IServiceCollection serviceCollection)
    {
        this.ServiceCollection = serviceCollection;
    }

    public void UseServiceProvider(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    public void AddPostDependencyBuildStep(Action step, ServerBuildStepPriority priority = ServerBuildStepPriority.Default)
    {
        this.postDependencyBuildSteps.Add(new PostDependencyBuildStep(step, priority));
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

    public void AddResourceServer<TResourceServer>(params object[] parameters)
        where TResourceServer : IResourceServer
    {
        AddBuildStep(server => server.AddResourceServer(server.Instantiate<TResourceServer>(parameters)));
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

    public void ConfigureServices(Action<IServiceCollection> action)
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
        foreach (var step in this.buildSteps.OrderByDescending(x => (int)x.Priority))
            step.Step(server);
    }

    public void LoadDependencies(IServiceCollection services)
    {
        foreach (var loader in this.dependecyLoaders)
            loader(services);

        foreach (var step in this.postDependencyBuildSteps.OrderBy(x => (int)x.Priority))
            step.Step();
    }
}
