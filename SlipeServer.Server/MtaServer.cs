using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.IdGeneration;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Loggers;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.PacketHandling;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.Repositories;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.ServerBuilders;
using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SlipeServer.Server;

public class MtaServer
{
    private readonly List<INetWrapper> netWrappers;
    private readonly List<IResourceServer> resourceServers;
    private readonly List<Resource> additionalResources;
    protected readonly PacketReducer packetReducer;
    protected readonly Dictionary<INetWrapper, Dictionary<uint, IClient>> clients;
    protected readonly ServiceCollection serviceCollection;
    protected readonly ServiceProvider serviceProvider;
    private readonly IElementRepository elementRepository;
    private readonly IElementIdGenerator? elementIdGenerator;
    private readonly IResourceProvider resourceProvider;
    private readonly RootElement root;
    private readonly Configuration configuration;

    private readonly Func<uint, INetWrapper, IClient>? clientCreationMethod;

    public string GameType { get; set; } = "unknown";
    public string MapName { get; set; } = "unknown";
    public string? Password { get; set; }
    public bool HasPassword => this.Password != null;

    public bool IsRunning { get; private set; }
    public DateTime StartDatetime { get; private set; }
    public TimeSpan Uptime => DateTime.Now - this.StartDatetime;

    public MtaServer(
        Action<ServerBuilder> builderAction,
        Func<uint, INetWrapper, IClient>? clientCreationMethod = null
    )
    {
        this.netWrappers = new();
        this.clients = new();
        this.clientCreationMethod = clientCreationMethod;
        this.resourceServers = new();
        this.additionalResources = new();

        this.root = new();
        this.serviceCollection = new();

        var builder = new ServerBuilder();
        builderAction(builder);

        this.configuration = builder.Configuration;
        this.Password = this.configuration.Password;
        this.SetupDependencies(services => builder.LoadDependencies(services));

        this.serviceProvider = this.serviceCollection.BuildServiceProvider();
        this.packetReducer = new(this.serviceProvider.GetRequiredService<ILogger>());

        foreach (var server in this.resourceServers)
            server.Start();

        this.elementRepository = this.serviceProvider.GetRequiredService<IElementRepository>();
        this.elementIdGenerator = this.serviceProvider.GetService<IElementIdGenerator>();
        this.resourceProvider = this.serviceProvider.GetRequiredService<IResourceProvider>();

        this.root.AssociateWith(this);

        builder.ApplyTo(this);
    }

    public void Start()
    {
        this.StartDatetime = DateTime.Now;

        foreach (var netWrapper in this.netWrappers)
        {
            netWrapper.Start();
        }

        this.IsRunning = true;
    }

    public void Stop()
    {
        foreach (var netWrapper in this.netWrappers)
        {
            netWrapper.Stop();
        }

        this.IsRunning = false;
    }

    public INetWrapper AddNetWrapper(string directory, string netDllPath, string host, ushort port, AntiCheatConfiguration? configuration = null)
    {
        var wrapper = CreateNetWrapper(directory, netDllPath, host, port);
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

    public void RegisterPacketHandler<T>(PacketId packetId, IPacketQueueHandler<T> queueHandler) where T : Packet, new()
        => this.packetReducer.RegisterPacketHandler(packetId, queueHandler);

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

    public object Instantiate(Type type, params object[] parameters) => ActivatorUtilities.CreateInstance(this.serviceProvider, type, parameters);
    public T Instantiate<T>() => ActivatorUtilities.CreateInstance<T>(this.serviceProvider);
    public T Instantiate<T>(params object[] parameters)
        => ActivatorUtilities.CreateInstance<T>(this.serviceProvider, parameters);

    public T GetService<T>() => this.serviceProvider.GetService<T>();
    public T GetRequiredService<T>() => this.serviceProvider.GetRequiredService<T>();

    public void BroadcastPacket(Packet packet)
    {
        packet.SendTo(this.clients.SelectMany(x => x.Value.Values));
    }

    public T AssociateElement<T>(T element) where T : Element
    {
        if (element != this.root && element.Parent == null)
            element.Parent = this.root;

        if (this.elementIdGenerator != null)
            element.Id = this.elementIdGenerator.GetId();

        this.ElementCreated?.Invoke(element);

        this.elementRepository.Add(element);
        element.Destroyed += (element) => this.elementRepository.Remove(element);

        return element;
    }

    public void AddResourceServer(IResourceServer resourceServer)
    {
        this.resourceServers.Add(resourceServer);
        resourceServer.Start();
    }

    public void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files)
    {
        resource.NetId = this.resourceProvider.ReserveNetId();
        this.additionalResources.Add(resource);
        foreach (var server in this.resourceServers)
            server.AddAdditionalResource(resource, files);
    }

    public void RemoveAdditionalResource(Resource resource)
    {
        this.additionalResources.Remove(resource);
        foreach (var server in this.resourceServers)
            server.RemoveAdditionalResource(resource);
    }

    public TResource GetAdditionalResource<TResource>()
        where TResource : Resource
    {
        return this.additionalResources
            .Where(x => x is TResource)
            .Select(x => (x as TResource)!)
            .Single();
    }

    public void ForAny<TElement>(Action<TElement> action)
        where TElement: Element
    {
        foreach (var element in this.elementRepository.GetByType<TElement>())
            action(element);

        this.ElementCreated += (element) =>
        {
            if (element is TElement tElement)
                action(tElement);
        };
    }

    protected virtual void SetupDependencies(Action<ServiceCollection>? dependencyCallback)
    {
        this.serviceCollection.AddSingleton<IElementRepository, RTreeCompoundElementRepository>();
        this.serviceCollection.AddSingleton<ILogger, DefaultLogger>();
        this.serviceCollection.AddSingleton<IResourceProvider, FileSystemResourceProvider>();
        this.serviceCollection.AddSingleton<IElementIdGenerator, RepositoryBasedElementIdGenerator>();
        this.serviceCollection.AddSingleton<IAseQueryService, AseQueryService>();
        this.serviceCollection.AddSingleton(typeof(ISyncHandlerMiddleware<>), typeof(BasicSyncHandlerMiddleware<>));

        this.serviceCollection.AddSingleton<GameWorld>();
        this.serviceCollection.AddSingleton<ChatBox>();
        this.serviceCollection.AddSingleton<ClientConsole>();
        this.serviceCollection.AddSingleton<DebugLog>();
        this.serviceCollection.AddSingleton<LuaValueMapper>();
        this.serviceCollection.AddSingleton<LuaEventService>();
        this.serviceCollection.AddSingleton<LatentPacketService>();
        this.serviceCollection.AddSingleton<ExplosionService>();
        this.serviceCollection.AddSingleton<FireService>();
        this.serviceCollection.AddSingleton<TextItemService>();
        this.serviceCollection.AddSingleton<WeaponConfigurationService>();
        this.serviceCollection.AddSingleton<CommandService>();

        this.serviceCollection.AddSingleton<HttpClient>();
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

    protected void RegisterNetWrapper(INetWrapper netWrapper)
    {
        netWrapper.PacketReceived += EnqueueIncomingPacket;
        this.clients[netWrapper] = new Dictionary<uint, IClient>();
    }

    private void EnqueueIncomingPacket(INetWrapper netWrapper, uint binaryAddress, PacketId packetId, byte[] data, uint? ping)
    {
        if (!this.clients[netWrapper].ContainsKey(binaryAddress))
        {
            var client = CreateClient(binaryAddress, netWrapper);
            AssociateElement(client.Player);

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
            if (this.clients[netWrapper].ContainsKey(binaryAddress))
            {
                var client = this.clients[netWrapper][binaryAddress];
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

    protected virtual IClient CreateClient(uint binaryAddress, INetWrapper netWrapper)
    {
        if (this.clientCreationMethod != null)
            return this.clientCreationMethod(binaryAddress, netWrapper);

        var player = new Player();
        player.Client = new Client(binaryAddress, netWrapper, player);
        return player.Client;
    }

    public void EnqueuePacketToClient(IClient client, PacketId packetId, byte[] data)
    {
        this.packetReducer.EnqueuePacket(client, packetId, data);
    }

    public virtual void HandlePlayerJoin(Player player) => PlayerJoined?.Invoke(player);
    public void HandleLuaEvent(LuaEvent luaEvent) => LuaEventTriggered?.Invoke(luaEvent);

    public void SetMaxPlayers(ushort slots)
    {
        this.configuration.MaxPlayerCount = slots;
        BroadcastPacket(new ServerInfoSyncPacket(slots));
    }

    public event Action<Element>? ElementCreated;
    public event Action<Player>? PlayerJoined;
    public event Action<IClient>? ClientConnected;
    public event Action<LuaEvent>? LuaEventTriggered;

}

public class MtaServer<TPlayer> : MtaServer
    where TPlayer : Player, new()
{
    public MtaServer(Action<ServerBuilder> builderAction)
        : base(builderAction)
    {

    }

    protected override void SetupDependencies(Action<ServiceCollection>? dependencyCallback)
    {
        base.SetupDependencies(dependencyCallback);
        this.serviceCollection.AddSingleton<MtaServer<TPlayer>>(this);
    }

    protected override IClient CreateClient(uint binaryAddress, INetWrapper netWrapper)
    {
        var player = new TPlayer();
        player.Client = new Client<TPlayer>(binaryAddress, netWrapper, player);
        return player.Client;
    }

    public override void HandlePlayerJoin(Player player)
    {
        base.HandlePlayerJoin(player);
        this.PlayerJoined?.Invoke((TPlayer)player);
    }

    public new event Action<TPlayer>? PlayerJoined;
}
