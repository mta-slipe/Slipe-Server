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
    public Configuration Configuration { get; private set; }
    private readonly List<Action<IServiceCollection>>? dependencyLoaders;

    public ServerBuilder(bool withDependencyLoaders = true)
    {
        this.Configuration = new();
        this.buildSteps = new();
        this.dependencyLoaders = withDependencyLoaders ? new() : null;
    }

    /// <summary>
    /// Specifies the `Configuration` instance to be used.
    /// this should generally be the first call in a builder since this configuration may be used in other build steps.
    /// </summary>
    /// <param name="configuration"></param>
    /// <exception cref="Exception"></exception>
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

    /// <summary>
    /// Adds an additional build step
    /// </summary>
    /// <param name="step">Build step</param>
    /// <param name="priority">Priority of the build step, determining in what order the build steps are executed. For fine control you can cast any integer to ServerBuildStepPriority</param>
    public void AddBuildStep(Action<MtaServer> step, ServerBuildStepPriority priority = ServerBuildStepPriority.Default)
    {
        this.buildSteps.Add(new ServerBuildStep(step, priority));
    }

    /// <summary>
    /// Adds a packet handler
    /// </summary>
    public void AddPacketHandler<TPacketQueueHandler, TPacketHandler, TPacket>(params object[] parameters)
        where TPacketQueueHandler : class, IPacketQueueHandler<TPacket>
        where TPacketHandler : IPacketHandler<TPacket>
        where TPacket : Packet, new()
    {
        AddBuildStep(server => server.RegisterPacketHandler<TPacket, TPacketQueueHandler, TPacketHandler>(parameters));
    }


    /// <summary>
    /// Adds a packet handler
    /// </summary>
    public void AddPacketHandler<TPacketHandler, TPacket>(params object[] parameters)
        where TPacketHandler : IPacketHandler<TPacket>
        where TPacket : Packet, new()
    {
        AddBuildStep(server => server.RegisterPacketHandler<TPacket, ScalingPacketQueueHandler<TPacket>, TPacketHandler>(parameters));
    }

    /// <summary>
    /// Adds a resource server
    /// </summary>
    public void AddResourceServer<TResourceServer>(params object[] parameters)
        where TResourceServer : IResourceServer
    {
        AddBuildStep(server => server.AddResourceServer(server.Instantiate<TResourceServer>(parameters)), ServerBuildStepPriority.High);
    }

    /// <summary>
    /// Will instantiate a certain type using the dependency injection container
    /// </summary>
    /// <typeparam name="T">Type to instantiate</typeparam>
    /// <param name="parameters">parameters to pass to the constructor that cannot be supplied by the dependency injection container</param>
    public void Instantiate<T>(params object[] parameters)
    {
        AddBuildStep(server => server.Instantiate<T>(parameters));
    }

    /// <summary>
    /// Will instantiate a certain type using the dependency injection container
    /// Will also keep a reference to the instantiated type to prevent it from being garbage collected
    /// </summary>
    /// <typeparam name="T">Type to instantiate</typeparam>
    /// <param name="parameters">parameters to pass to the constructor that cannot be supplied by the dependency injection container</param>
    public void InstantiatePersistent<T>(params object[] parameters)
    {
        AddBuildStep(server => server.InstantiatePersistent<T>(parameters));
    }

    /// <summary>
    /// Will instantiate a certain type using the dependency injection container with scoped lifetime
    /// Will also keep a reference to the instantiated type to prevent it from being garbage collected
    /// </summary>
    /// <typeparam name="T">Type to instantiate</typeparam>
    /// <param name="parameters">parameters to pass to the constructor that cannot be supplied by the dependency injection container</param>
    public void InstantiateScopedPersistent<T>(params object[] parameters)
    {
        AddBuildStep(server => server.InstantiateScopedPersistent<T>(parameters));
    }
    
    /// <summary>
    /// Will instantiate a certain type using the dependency injection container
    /// Will also keep a reference to the instantiated type to prevent it from being garbage collected
    /// </summary>
    /// <param name="type">The type to instiantiate</param>
    /// <param name="parameters">parameters to pass to the constructor that cannot be supplied by the dependency injection container</param>
    public void InstantiatePersistent(Type type, params object[] parameters)
    {
        AddBuildStep(server => server.InstantiatePersistent(type, parameters));
    }

    /// <summary>
    /// Adds a behaviour class to the server, purely for semantics otherwise identical to `InstiantiatePersistent` 
    /// </summary>
    public void AddBehaviour<T>(params object[] parameters)
    {
        InstantiatePersistent<T>(parameters);
    }

    /// <summary>
    /// Adds a game logic class to the server, purely for semantics otherwise identical to `InstiantiatePersistent` 
    /// </summary>
    public void AddLogic<T>(params object[] parameters)
    {
        InstantiatePersistent<T>(parameters);
    }

    /// <summary>
    /// Adds a game scoped logic class to the server, purely for semantics otherwise identical to `InstantiateScopedPersistent` 
    /// </summary>
    public void AddScopedLogic<T>(params object[] parameters)
    {
        InstantiateScopedPersistent<T>(parameters);
    }

    /// <summary>
    /// Adds a game logic class to the server, purely for semantics otherwise identical to `InstiantiatePersistent` 
    /// </summary>
    public void AddLogic(Type type, params object[] parameters)
    {
        InstantiatePersistent(type, parameters);
    }

    /// <summary>
    /// Configures additional dependencies for the dependecy injection container
    /// </summary>
    /// <param name="action"></param>
    public void ConfigureServices(Action<IServiceCollection> action)
    {
        if (this.dependencyLoaders == null)
            throw new NotSupportedException();

        this.dependencyLoaders.Add(action);
    }

    /// <summary>
    /// Adds a networking interface
    /// </summary>
    /// <param name="directory">directory to run in</param>
    /// <param name="dllPath">path to the net.dll, relative to the directory</param>
    /// <param name="host">host ip for the server</param>
    /// <param name="port">UDP port for incoming traffic to the server, this is what port players connect to</param>
    /// <param name="antiCheatConfiguration">anti cheat configuration to apply on this networking interface</param>
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

    /// <summary>
    /// Applies the build steps to the MTA server
    /// </summary>
    /// <param name="server"></param>
    public void ApplyTo(MtaServer server)
    {
        var steps = this.buildSteps
            .OrderByDescending(x => (int)x.Priority)
            .ToArray();

        foreach (var step in steps)
            step.Step(server);
    }

    /// <summary>
    /// Loads additional dependencies to the dependency injection service collection
    /// </summary>
    /// <param name="services"></param>
    public void LoadDependencies(IServiceCollection services)
    {
        if(this.dependencyLoaders != null)
            foreach (var loader in this.dependencyLoaders)
                loader(services);
    }
}
