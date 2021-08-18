using SlipeServer.Packets.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.Net;
using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Elements.IdGeneration;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;
using SlipeServer.Server.Resources.ResourceServing;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Server.ServerOptions;

namespace SlipeServer.Server
{
    public class MtaServer
    {
        private readonly List<INetWrapper> netWrappers;
        private readonly IResourceServer resourceServer;
        protected readonly PacketReducer packetReducer;
        private readonly Dictionary<INetWrapper, Dictionary<uint, Client>> clients;
        private readonly ServiceCollection serviceCollection;
        private readonly ServiceProvider serviceProvider;
        private readonly IElementRepository elementRepository;
        private readonly IElementIdGenerator? elementIdGenerator;
        private readonly RootElement root;
        private readonly Configuration configuration;

        private readonly Func<uint, INetWrapper, Client>? clientCreationMethod;

        public string GameType { get; set; } = "unknown";
        public string MapName { get; set; } = "unknown";
        public string Password { get; set; } = "";
        public bool HasPassword => this.Password != "";

        public bool IsRunning { get; private set; }
        public DateTime StartDatetime { get; private set; }
        public TimeSpan Uptime => DateTime.Now - this.StartDatetime;

        public MtaServer(
            Configuration? configuration = null,
            Action<ServiceCollection>? dependencyCallback = null,
            Func<uint, INetWrapper, Client>? clientCreationMethod = null
        )
        {
            this.netWrappers = new List<INetWrapper>();

            this.configuration = configuration ?? new Configuration();
            this.clientCreationMethod = clientCreationMethod;
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(this.configuration, new ValidationContext(this.configuration), validationResults, true))
            {
                string invalidProperties = string.Join("\r\n\t", validationResults.Select(r => r.ErrorMessage));
                throw new Exception($"An error has occurred while parsing configuration parameters:\r\n {invalidProperties}");
            }

            this.root = new RootElement();

            this.serviceCollection = new ServiceCollection();
            this.SetupDependencies(dependencyCallback);
            this.serviceProvider = this.serviceCollection.BuildServiceProvider();

            this.resourceServer = this.serviceProvider.GetRequiredService<IResourceServer>();
            this.resourceServer.Start();

            this.elementRepository = this.serviceProvider.GetRequiredService<IElementRepository>();
            this.elementIdGenerator = this.serviceProvider.GetService<IElementIdGenerator>();

            this.root.AssociateWith(this);

            this.packetReducer = new PacketReducer(this.serviceProvider.GetRequiredService<ILogger>());
            this.clients = new Dictionary<INetWrapper, Dictionary<uint, Client>>();
        }

        public MtaServer(
            string directory,
            string netDllPath,
            Configuration? configuration = null,
            Action<ServiceCollection>? dependencyCallback = null,
            Func<uint, INetWrapper, Client>? clientCreationMethod = null
        ) : this(configuration, dependencyCallback, clientCreationMethod)
        {
            this.AddNetWrapper(directory, netDllPath, this.configuration.Host, this.configuration.Port, this.configuration.AntiCheat);
        }

        public MtaServer(
            Action<ServerBuilder> builderAction,
            Configuration configuration,
            Action<ServiceCollection>? dependencyCallback = null,
            Func<uint, INetWrapper, Client>? clientCreationMethod = null
        ) : this(configuration, dependencyCallback, clientCreationMethod)
        {
            var builder = new ServerBuilder(configuration);
            builderAction(builder);
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

        public void RegisterPacketQueueHandler(PacketId packetId, IQueueHandler queueHandler)
        {
            this.packetReducer.RegisterQueueHandler(packetId, queueHandler);
        }

        public void RegisterPacketQueueHandler(IQueueHandler queueHandler)
        {
            foreach (var packetId in queueHandler.SupportedPacketIds)
                this.RegisterPacketQueueHandler(packetId, queueHandler);
        }

        public void RegisterPacketQueueHandler<T>(params object[] parameters) where T: IQueueHandler
        {
            this.RegisterPacketQueueHandler(this.Instantiate<T>(parameters));
        }

        public object Instantiate(Type type) => ActivatorUtilities.CreateInstance(this.serviceProvider, type);
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
            if (this.elementIdGenerator != null)
            {
                element.Id = this.elementIdGenerator.GetId();
            }
            this.elementRepository.Add(element);
            element.Destroyed += (element) => this.elementRepository.Remove(element);

            this.ElementCreated?.Invoke(element);

            if (element != this.root)
                element.Parent = this.root;

            return element;
        }

        private void SetupDependencies(Action<ServiceCollection>? dependencyCallback)
        {
            this.serviceCollection.AddSingleton<IElementRepository, CompoundElementRepository>();
            this.serviceCollection.AddSingleton<ILogger, DefaultLogger>();
            this.serviceCollection.AddSingleton<IResourceServer, BasicHttpServer>();
            this.serviceCollection.AddSingleton<IElementIdGenerator, RepositoryBasedElementIdGenerator>();
            this.serviceCollection.AddSingleton<IAseQueryService, AseQueryService>();
            this.serviceCollection.AddSingleton(typeof(ISyncHandlerMiddleware<>), typeof(BasicSyncHandlerMiddleware<>));

            this.serviceCollection.AddSingleton<GameWorld>();
            this.serviceCollection.AddSingleton<ChatBox>();
            this.serviceCollection.AddSingleton<ClientConsole>();
            this.serviceCollection.AddSingleton<DebugLog>();
            this.serviceCollection.AddSingleton<LuaEventService>();
            this.serviceCollection.AddSingleton<ExplosionService>();
            this.serviceCollection.AddSingleton<FireService>();
            this.serviceCollection.AddSingleton<TextItemService>();

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
            this.clients[netWrapper] = new Dictionary<uint, Client>();
        }

        private void EnqueueIncomingPacket(INetWrapper netWrapper, uint binaryAddress, PacketId packetId, byte[] data, uint? ping)
        {
            if (!this.clients[netWrapper].ContainsKey(binaryAddress))
            {
                var client = this.clientCreationMethod?.Invoke(binaryAddress, netWrapper) ??
                    new Client(binaryAddress, netWrapper); 
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
                    this.clients[netWrapper].Remove(binaryAddress);
                }
            }
        }

        public void HandlePlayerJoin(Player player) => PlayerJoined?.Invoke(player);
        public void HandleLuaEvent(LuaEvent luaEvent) => LuaEventTriggered?.Invoke(luaEvent);

        public void SetMaxPlayers(ushort slots)
        {
            this.configuration.MaxPlayerCount = slots;
            BroadcastPacket(new ServerInfoSyncPacket(slots));
        }

        public event Action<Element>? ElementCreated;
        public event Action<Player>? PlayerJoined;
        public event Action<Client>? ClientConnected;
        public event Action<LuaEvent>? LuaEventTriggered;

    }
}
