using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Bans;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.IdGeneration;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Loggers;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.PacketHandling;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Handlers.Connection;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.ServerBuilders;
using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server;

/// <summary>
/// A highly-configurable implementation of an MTA Server. 
/// </summary>
public class MtaServer
{
    private readonly List<INetWrapper> netWrappers;
    protected readonly List<IResourceServer> resourceServers;
    private readonly List<Resource> additionalResources;
    protected PacketReducer packetReducer;
    protected readonly Dictionary<INetWrapper, Dictionary<ulong, IClient>> clients;
    protected readonly IServiceCollection? serviceCollection;
    protected readonly IServiceProvider serviceProvider;
    protected readonly IElementCollection elementCollection;
    private readonly IElementIdGenerator? elementIdGenerator;
    protected IResourceProvider? resourceProvider;
    private readonly RootElement root;
    private readonly Configuration configuration;

    private readonly Func<ulong, INetWrapper, IClient>? clientCreationMethod;

    private readonly HashSet<object> persistentInstances;

    /// <summary>
    /// Game type, as shown in the server browser
    /// </summary>
    public string GameType { get; set; } = "unknown";

    /// <summary>
    /// Map name, as shown in the server browser
    /// </summary>
    public string MapName { get; set; } = "unknown";

    /// <summary>
    /// Current server password, to be entered when connecting
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Indicates whether a password is set
    /// </summary>
    public bool HasPassword => this.Password != null;

    /// <summary>
    /// Indicates whether the server is currently accepting incoming packets
    /// </summary>
    public bool IsRunning { get; protected set; }

    /// <summary>
    /// The timestamp the server was started at
    /// </summary>
    public DateTime StartDatetime { get; protected set; }

    /// <summary>
    /// The amount of time since the server has been started
    /// </summary>
    public TimeSpan Uptime => DateTime.Now - this.StartDatetime;

    /// <summary>
    /// Returns the service provider, which can be used to instantiate / inject services
    /// </summary>
    public IServiceProvider Services => this.serviceProvider;

    public IEnumerable<Player> Players => this.elementCollection.GetByType<Player>();
    public RootElement RootElement => this.root;
    public Configuration Configuration => this.configuration;

    public MtaServer(
        Action<ServerBuilder> builderAction,
        Func<ulong, INetWrapper, IClient>? clientCreationMethod = null
    )
    {
        this.netWrappers = new();
        this.clients = new();
        this.clientCreationMethod = clientCreationMethod;
        this.resourceServers = new();
        this.additionalResources = new();
        this.persistentInstances = new();

        this.root = new();
        this.serviceCollection = new ServiceCollection();

        var builder = new ServerBuilder();
        builderAction(builder);

        this.configuration = builder.Configuration;
        this.Password = this.configuration.Password;
        this.SetupDependencies(builder.LoadDependencies);

        this.serviceProvider = this.serviceCollection.BuildServiceProvider();

        this.elementCollection = this.serviceProvider.GetRequiredService<IElementCollection>();
        this.elementIdGenerator = this.serviceProvider.GetService<IElementIdGenerator>();
        this.packetReducer = new(this.serviceProvider.GetRequiredService<ILogger>());

        this.root.AssociateWith(this);

        builder.ApplyTo(this);
    }

    public Action? BuildFinalizer { get; }

    public MtaServer(IServiceProvider serviceProvider, Action<ServerBuilder> builderAction)
    {
        this.netWrappers = new();
        this.clients = new();
        this.clientCreationMethod = serviceProvider.GetService<Func<ulong, INetWrapper, IClient>>();
        this.resourceServers = new();
        this.additionalResources = new();
        this.persistentInstances = new();

        this.root = new();

        var builder = new ServerBuilder();
        builderAction(builder);

        this.configuration = builder.Configuration;
        this.Password = this.configuration.Password;

        this.serviceProvider = serviceProvider;

        this.elementCollection = this.serviceProvider.GetRequiredService<IElementCollection>();
        this.elementIdGenerator = this.serviceProvider.GetService<IElementIdGenerator>();
        this.packetReducer = new(this.serviceProvider.GetRequiredService<ILogger>());

        this.root.AssociateWith(this);

        this.BuildFinalizer = () => builder.ApplyTo(this);
    }

    /// <summary>
    /// Starts the networking interfaces, allowing clients to connect and packets to be sent out to clients.
    /// </summary>
    public virtual void Start()
    {
        this.StartDatetime = DateTime.Now;

        foreach (var netWrapper in this.netWrappers)
        {
            netWrapper.Start();
        }

        this.resourceProvider?.Refresh();

        foreach (var server in this.resourceServers)
            server.Start();

        this.IsRunning = true;

        this.Started?.Invoke(this);
    }

    /// <summary>
    /// Stops the networking interfaces.
    /// </summary>
    public virtual void Stop()
    {
        foreach (var player in this.elementCollection.GetByType<Player>())
            player.Kick(PlayerDisconnectType.SHUTDOWN);

        foreach (var player in this.elementCollection.GetAll())
            player.Destroy();

        foreach (var server in this.resourceServers)
            server.Stop();

        foreach (var netWrapper in this.netWrappers)
        {
            netWrapper.Stop();
        }

        this.IsRunning = false;

        this.Stopped?.Invoke(this);
    }

    /// <summary>
    /// Adds a new networking interface using MTA's net.dll
    /// </summary>
    /// <param name="directory">directory to run in</param>
    /// <param name="netDllPath">path to the net.dll, relative to the directory</param>
    /// <param name="host">host ip for the server</param>
    /// <param name="port">UDP port for incoming traffic to the server, this is what port players connect to</param>
    /// <param name="configuration">anti cheat configuration to apply on this networking interface</param>
    /// <returns></returns>
    public INetWrapper AddNetWrapper(string directory, string netDllPath, string host, ushort port, AntiCheatConfiguration? configuration = null)
    {
        var wrapper = CreateNetWrapper(directory, netDllPath, host, port);
        this.netWrappers.Add(wrapper);

        ConfigureAntiCheat(wrapper, configuration ?? new AntiCheatConfiguration());

        if (this.IsRunning)
            wrapper.Start();

        return wrapper;
    }

    /// <summary>
    /// Adds an arbitrary networking interface
    /// </summary>
    /// <param name="wrapper">the networking interface</param>
    /// <param name="configuration">anti cheat configuration to apply on this networking interface</param>
    /// <returns></returns>
    public INetWrapper AddNetWrapper(INetWrapper wrapper, AntiCheatConfiguration? configuration = null)
    {
        this.netWrappers.Add(wrapper);

        ConfigureAntiCheat(wrapper, configuration ?? new AntiCheatConfiguration());

        if (this.IsRunning)
            wrapper.Start();

        return wrapper;
    }


    private void ConfigureAntiCheat(INetWrapper netWrapper, AntiCheatConfiguration configuration)
    {
        netWrapper.SetAntiCheatConfig(
            configuration.DisabledAntiCheat,
            configuration.HideAntiCheat,
            configuration.AllowGta3ImgMods,
            configuration.EnableSpecialDetections,
            configuration.FileChecks
        );
    }


    /// <summary>
    /// Registers a packet handler, to handle incoming packets from clients
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="packetId">The packet ID to handle, this identifies which packet types should be handled by this handler</param>
    /// <param name="queueHandler">The packet handler in question</param>
    public void RegisterPacketHandler<T>(PacketId packetId, IPacketQueueHandler<T> queueHandler) where T : Packet, new()
        => this.packetReducer.RegisterPacketHandler(packetId, queueHandler);

    /// <summary>
    /// Registers a packet handler, to handle incoming packets from clients
    /// </summary>
    /// <typeparam name="TPacket">The type of packet ot handle</typeparam>
    /// <typeparam name="TPacketQueueHandler">The type of packet queue handler to handle the queue of packets</typeparam>
    /// <typeparam name="TPacketHandler">The type of packet handler to instantiate</typeparam>
    /// <param name="parameters">Any parameters to pass to the constructor of the packet handler that are not injected by the dependency injection container</param>
    public void RegisterPacketHandler<TPacket, TPacketQueueHandler, TPacketHandler>(params object[] parameters)
        where TPacket : Packet, new()
        where TPacketQueueHandler : class, IPacketQueueHandler<TPacket>
        where TPacketHandler : IPacketHandler<TPacket>
    {
        var packetHandler = this.Instantiate<TPacketHandler>();
        var queueHandler = this.Instantiate(
            typeof(TPacketQueueHandler),
            Array.Empty<object>()
                .Concat(new object[] { packetHandler })
                .Concat(parameters)
                .ToArray()
            ) as TPacketQueueHandler;
        this.packetReducer.RegisterPacketHandler(packetHandler.PacketId, queueHandler!);
    }

    /// <summary>
    /// Registers a packet handler, to handle incoming packets from clients using default ( ScalingPacketQueueHandler ) handler
    /// </summary>
    /// <typeparam name="TPacketHandler"></typeparam>
    /// <typeparam name="TPacket"></typeparam>
    /// <param name="parameters"></param>
    public void RegisterPacketHandler<TPacketHandler, TPacket>(params object[] parameters)
        where TPacket : Packet, new()
        where TPacketHandler : IPacketHandler<TPacket>
    {
        RegisterPacketHandler<TPacket, ScalingPacketQueueHandler<TPacket>, TPacketHandler>();
    }
        /// <summary>
        /// Instantiates a type using the dependency injection container
        /// </summary>
        /// <param name="type">The type to instiantiate</param>
        /// <param name="parameters">Any constructor parameters that are not supplied by the dependency injection container</param>
        /// <returns></returns>
        public object Instantiate(Type type, params object[] parameters)
        => ActivatorUtilities.CreateInstance(this.serviceProvider, type, parameters);

    /// <summary>
    /// Instantiates a type using the dependency injection container
    /// </summary>
    /// <typeparam name="T">The type to instiantiate</typeparam>
    /// <param name="parameters">Any constructor parameters that are not supplied by the dependency injection container</param>
    /// <returns></returns>
    public T Instantiate<T>(params object[] parameters)
        => ActivatorUtilities.CreateInstance<T>(this.serviceProvider, parameters);

    /// <summary>
    /// Instantiates a type using the dependency injection container, and keeps a reference to it on the MTA server, making sure it does not get garbage collected
    /// </summary>
    /// <param name="type">The type to instiantiate</param>
    /// <param name="parameters">Any constructor parameters that are not supplied by the dependency injection container</param>
    /// <returns></returns>
    public object InstantiatePersistent(Type type, params object[] parameters)
    {
        var instance = this.Instantiate(type, parameters);
        this.persistentInstances.Add(instance);
        return instance;
    }

    /// <summary>
    /// Instantiates a type using the dependency injection container, and keeps a reference to it on the MTA server, making sure it does not get garbage collected
    /// </summary>
    /// <typeparam name="T">The type to instiantiate</typeparam>
    /// <param name="parameters">Any constructor parameters that are not supplied by the dependency injection container</param>
    /// <returns></returns>
    public T InstantiatePersistent<T>(params object[] parameters)
    {
        var instance = this.Instantiate<T>(parameters);
        if (instance == null)
            throw new Exception($"Unable to instantiate {typeof(T)}");

        this.persistentInstances.Add(instance);
        return instance;
    }

    /// <summary>
    /// Instantiates a type using the dependency injection container with scoped lifetime, and keeps a reference to it on the MTA server, making sure it does not get garbage collected
    /// </summary>
    /// <typeparam name="T">The type to instiantiate</typeparam>
    /// <param name="parameters">Any constructor parameters that are not supplied by the dependency injection container</param>
    /// <returns></returns>
    public T InstantiateScopedPersistent<T>(params object[] parameters)
    {
        var instance = this.InstantiateScoped<T>(parameters);
        if (instance == null)
            throw new Exception($"Unable to instantiate {typeof(T)}");

        this.persistentInstances.Add(instance);
        return instance;
    }

    /// <summary>
    /// Instantiates a type using the dependency injection container, in a newly created scope.
    /// </summary>
    /// <param name="type">The type to instiantiate</param>
    /// <param name="parameters">Any constructor parameters that are not supplied by the dependency injection container</param>
    /// <returns></returns>
    public object InstantiateScoped(Type type, params object[] parameters)
    {
        var scope = this.serviceProvider.CreateScope();
        return ActivatorUtilities.CreateInstance(scope.ServiceProvider, type, parameters);
    }

    /// <summary>
    /// Instantiates a type using the dependency injection container, in a newly created scope.
    /// </summary>
    /// <typeparam name="T">The type to instiantiate</typeparam>
    /// <param name="parameters">Any constructor parameters that are not supplied by the dependency injection container</param>
    /// <returns></returns>
    public T InstantiateScoped<T>(params object[] parameters)
    {
        var scope = this.serviceProvider.CreateScope();
        return ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider, parameters);
    }

    /// <summary>
    /// Gets a registered service from the dependency injection conatiner
    /// </summary>
    /// <returns></returns>
    public T? GetService<T>() => this.serviceProvider.GetService<T>();

    /// <summary>
    /// Gets a registered service from the dependency injection conatiner, throwing an exception if there is no registered service for the specified type
    /// </summary>
    /// <returns></returns>
    public T GetRequiredService<T>() where T : notnull => this.serviceProvider.GetRequiredService<T>();

    /// <summary>
    /// Gets a registered service from the dependency injection conatiner, throwing an exception if there is no registered service for the specified type
    /// </summary>
    /// <returns></returns>
    public T GetRequiredServiceScoped<T>() where T : notnull
    {
        var scope = this.serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Sends a packet to all players on the server.
    /// </summary>
    /// <param name="packet"></param>
    public void BroadcastPacket(Packet packet)
    {
        packet.SendTo(this.clients.SelectMany(x => x.Value.Values));
    }

    /// <summary>
    /// Associates an element with the entire server, meaning any player that connects to the server will be made aware of the element
    /// and changes to the element will be relayed to all the players connected to the server.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <returns>Returns the element, allowing for method chaining</returns>
    public T AssociateElement<T>(T element) where T : Element
    {
        if (element != this.root && element.Parent == null)
            element.Parent = this.root;

        if (this.elementIdGenerator != null && element.Id == default)
            element.Id = (ElementId)this.elementIdGenerator.GetId();

        this.ElementCreated?.Invoke(element);

        this.elementCollection.Add(element);
        element.Destroyed += this.elementCollection.Remove;

        return element;
    }

    /// <summary>
    /// Removes an element from being associated with the entire server, meaning the element will no longer be sync'd to all players
    /// </summary>
    /// <param name="element"></param>
    public void RemoveElement(Element element)
    {
        element.Destroy();
        element.Destroyed -= this.elementCollection.Remove;
    }

    /// <summary>
    /// Adds a resource server, this is responsible for providing the files for client side resources to clients, over HTTP.
    /// </summary>
    public void AddResourceServer(IResourceServer resourceServer)
    {
        this.resourceServers.Add(resourceServer);
        resourceServer.Start();
    }

    /// <summary>
    /// Adds an additional resource to the server, which then gets registered with all resource servers.
    /// </summary>
    public void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files)
    {
        if(this.resourceProvider == null)
        {
            this.resourceProvider = this.serviceProvider.GetRequiredService<IResourceProvider>();
        }
        resource.NetId = this.resourceProvider.ReserveNetId();
        this.additionalResources.Add(resource);
        foreach (var server in this.resourceServers)
            server.AddAdditionalResource(resource, files);
    }

    /// <summary>
    /// Removes a registered additional resource.
    /// </summary>
    public void RemoveAdditionalResource(Resource resource)
    {
        this.additionalResources.Remove(resource);
        foreach (var server in this.resourceServers)
            server.RemoveAdditionalResource(resource);
    }

    /// <summary>
    /// Gets a registered additional resource of a certain type.
    /// </summary>
    public TResource GetAdditionalResource<TResource>()
        where TResource : Resource
    {
        return this.additionalResources
            .Where(x => x is TResource)
            .Select(x => (x as TResource)!)
            .Single();
    }

    /// <summary>
    /// Executes an action for every single element of a specific type on the server. 
    /// This includes both currently existing elements, and elements to be created in the future.
    /// </summary>
    /// <typeparam name="TElement">The type of element to execute the action for</typeparam>
    /// <param name="action">The action to be executed for every element of the type</param>
    public void ForAny<TElement>(Action<TElement> action)
        where TElement : Element
    {
        foreach (var element in this.elementCollection.GetByType<TElement>())
            action(element);

        this.ElementCreated += (element) =>
        {
            if (element is TElement tElement)
                action(tElement);
        };
    }

    protected virtual void SetupDependencies(Action<IServiceCollection>? dependencyCallback)
    {
        if (this.serviceCollection == null)
            throw new NotSupportedException();

        this.serviceCollection.AddDefaultMtaServerServices();
        this.serviceCollection.AddSingleton<Configuration>(this.configuration);
        this.serviceCollection.AddSingleton<RootElement>(this.root);
        this.serviceCollection.AddSingleton<MtaServer>(this);

        dependencyCallback?.Invoke(this.serviceCollection);
    }

    private INetWrapper CreateNetWrapper(string directory, string netDllPath, string host, ushort port)
    {
        INetWrapper netWrapper = new NetWrapper(directory, netDllPath, host, port);
        RegisterNetWrapper(netWrapper);
        return netWrapper;
    }

    /// <summary>
    /// Registers a networking interface, meaning any packets received by this will be handled by registered packet handlers.
    /// </summary>
    /// <param name="netWrapper"></param>
    public void RegisterNetWrapper(INetWrapper netWrapper)
    {
        netWrapper.PacketReceived += EnqueueIncomingPacket;
        this.clients[netWrapper] = new();
    }

    public string GetPingStatus()
    {
        return this.clients
            .OrderBy(x => x.Value.Count)
            .First()
            .Key
            .GetPingStatus();
            //.PadRight(32, 'P')
            //.Substring(0, 32);
    }

    private void EnqueueIncomingPacket(INetWrapper netWrapper, ulong binaryAddress, PacketId packetId, byte[] data, uint? ping)
    {
        if (!this.clients[netWrapper].ContainsKey(binaryAddress))
        {
            var client = CreateClient(binaryAddress, netWrapper);
            client.Player.AssociateWith(this);

            this.clients[netWrapper][binaryAddress] = client;
            ClientConnected?.Invoke(client);
        }

        if (ping != null)
            this.clients[netWrapper][binaryAddress].Ping = ping.Value;

        this.packetReducer.EnqueuePacket(this.clients[netWrapper][binaryAddress], packetId, data);

        if (
            packetId == PacketId.PACKET_ID_PLAYER_QUIT ||
            packetId == PacketId.PACKET_ID_PLAYER_TIMEOUT ||
            packetId == PacketId.PACKET_ID_PLAYER_NO_SOCKET
        )
        {
            if (this.clients[netWrapper].TryGetValue(binaryAddress, out var client))
            {
                client.IsConnected = false;
                var quitReason = packetId switch
                {
                    PacketId.PACKET_ID_PLAYER_QUIT => QuitReason.Quit,
                    PacketId.PACKET_ID_PLAYER_TIMEOUT => QuitReason.Timeout,
                    PacketId.PACKET_ID_PLAYER_NO_SOCKET => QuitReason.Timeout,
                    _ => throw new NotImplementedException()
                };
                client.Player.TriggerDisconnected(quitReason);
                client.SetDisconnected();
                this.clients[netWrapper].Remove(binaryAddress);
            }
        }
    }

    protected virtual IClient CreateClient(ulong binaryAddress, INetWrapper netWrapper)
    {
        if (this.clientCreationMethod != null)
            return this.clientCreationMethod(binaryAddress, netWrapper);

        var player = new Player();
        player.Client = new Client(binaryAddress, netWrapper, player);
        return player.Client;
    }

    /// <summary>
    /// Enqueues a packet as if it were received from a specific client
    /// </summary>
    public void EnqueuePacketToClient(IClient client, PacketId packetId, byte[] data)
    {
        this.packetReducer.EnqueuePacket(client, packetId, data);
    }

    /// <summary>
    /// Handles a player joining the server, and triggers the appropriate event.
    /// This method is generally intended to be called from packet handlers.
    /// </summary>
    /// <param name="player"></param>
    public virtual void HandlePlayerJoin(Player player)
    {
        player.TriggerJoined();
        PlayerJoined?.Invoke(player);
    }

    /// <summary>
    /// Handles a lua event, and triggers the appropriate event.
    /// This method is generally intended to be called from packet handlers.
    /// </summary>
    /// <param name="luaEvent"></param>
    public void HandleLuaEvent(LuaEvent luaEvent) => LuaEventTriggered?.Invoke(luaEvent);

    /// <summary>
    /// Sets the maximum amount of players on the server
    /// </summary>
    /// <param name="slots"></param>
    public void SetMaxPlayers(ushort slots)
    {
        this.configuration.MaxPlayerCount = slots;
        BroadcastPacket(new ServerInfoSyncPacket(slots));
        this.MaxPlayerCountChanged?.Invoke(slots);
    }

    /// <summary>
    /// Creates an MTA Server.
    /// </summary>
    /// <param name="builderAction">Action that allows you to configure the server</param>
    /// <returns></returns>
    public static MtaServer Create(IServiceProvider serviceProvider, Action<ServerBuilder> builderAction)
        => new(serviceProvider, builderAction);

    /// <summary>
    /// Creates an MTA server using a specific type for connecting players
    /// </summary>
    /// <typeparam name="TPlayer">The type to use for connecting players</typeparam>
    /// <param name="builderAction">Action that allows you to configure the server</param>
    /// <returns></returns>
    public static MtaServer<TPlayer> Create<TPlayer>(Action<ServerBuilder> builderAction) where TPlayer : Player, new()
        => new MtaNewPlayerServer<TPlayer>(builderAction);

    /// <summary>
    /// Creates an MTA server using a specific type for connecting players.
    /// This player type will be instantiated using the dependency injection container, allowing you to inject dependencies into the TPlayer class.
    /// </summary>
    /// <typeparam name="TPlayer"></typeparam>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    public static MtaServer<TPlayer> CreateWithDiSupport<TPlayer>(Action<ServerBuilder> builderAction) where TPlayer : Player
        => new MtaDiPlayerServer<TPlayer>(builderAction);

    /// <summary>
    /// Triggered when any element is created on the server through the .AssociateElement method
    /// </summary>
    public event Action<Element>? ElementCreated;

    /// <summary>
    /// Triggered when any player joins the server
    /// </summary>
    public event Action<Player>? PlayerJoined;

    /// <summary>
    /// Triggered when a client connects to the server
    /// </summary>
    public event Action<IClient>? ClientConnected;

    /// <summary>
    /// Triggered when a lua event has been triggered by a client
    /// </summary>
    public event Action<LuaEvent>? LuaEventTriggered;

    /// <summary>
    /// Triggered when max player count changes
    /// </summary>
    public event Action<ushort>? MaxPlayerCountChanged;

    /// <summary>
    /// Triggered when the server started
    /// </summary>
    public event Action<MtaServer>? Started;

    /// <summary>
    /// Triggered when the server stopped
    /// </summary>
    public event Action<MtaServer>? Stopped;
}

/// <summary>
/// Base class for any MtaServer that supports an alternative player class.
/// This class can not be instantiated
/// </summary>
/// <typeparam name="TPlayer">The player type</typeparam>
public abstract class MtaServer<TPlayer> : MtaServer where TPlayer : Player
{
    public MtaServer(IServiceProvider serviceProvider, Action<ServerBuilder> builderAction) : base(serviceProvider, builderAction) { }

    public MtaServer(Action<ServerBuilder> builderAction) : base(builderAction) { }

    protected override void SetupDependencies(Action<IServiceCollection>? dependencyCallback)
    {
        if (this.serviceCollection == null)
            throw new NotSupportedException();

        base.SetupDependencies(dependencyCallback);
        this.serviceCollection.AddSingleton<MtaServer<TPlayer>>(this);
    }

    public override void HandlePlayerJoin(Player player)
    {
        base.HandlePlayerJoin(player);
        this.PlayerJoined?.Invoke((TPlayer)player);
    }

    public new event Action<TPlayer>? PlayerJoined;
}

/// <summary>
/// A highly configurable implementation of an MTA server, with a custom player class. This class is required to have a parameterless constructor.
/// Instaces of this class can be created using `MtaServer.Create&lt;TPlayer&gt;`
/// </summary>
/// <typeparam name="TPlayer">The player type</typeparam>
public class MtaNewPlayerServer<TPlayer> : MtaServer<TPlayer> where TPlayer : Player, new()
{
    public MtaNewPlayerServer(IServiceProvider serviceProvider, Action<ServerBuilder> builderAction) : base(serviceProvider, builderAction) { }
    internal MtaNewPlayerServer(Action<ServerBuilder> builderAction) : base(builderAction) { }

    protected override IClient CreateClient(ulong binaryAddress, INetWrapper netWrapper)
    {
        var player = new TPlayer();
        player.Client = new Client<TPlayer>(binaryAddress, netWrapper, player);
        return player.Client;
    }
}

/// <summary>
/// A highly-configurable implementation of an MTA Server, with support for dependency injection for the player class.
/// Instances of this class can be created using `MtaServer.CreateWithDiSupport()`
/// </summary>
public class MtaDiPlayerServer<TPlayer> : MtaServer<TPlayer> where TPlayer : Player
{
    public MtaDiPlayerServer(Action<ServerBuilder> builderAction) : base(builderAction) { }

    public MtaDiPlayerServer(IServiceProvider serviceProvider, Action<ServerBuilder> builderAction) : base(serviceProvider, builderAction) { }

    protected override IClient CreateClient(ulong binaryAddress, INetWrapper netWrapper)
    {
        var player = this.Instantiate<TPlayer>();
        player.Client = new Client<TPlayer>(binaryAddress, netWrapper, player);
        return player.Client;
    }
}
