using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MtaServer.Net;
using MtaServer.Packets;
using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using MtaServer.Server.Extensions;
using MtaServer.Server.PacketHandling;
using MtaServer.Server.Repositories;
using MtaServer.Server.ResourceServing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace MtaServer.Server
{
    public class MtaServer
    {
        private readonly NetWrapper netWrapper;
        private readonly IResourceServer resourceServer;
        private readonly PacketReducer packetReducer;
        private readonly Dictionary<NetWrapper, Dictionary<uint, Client>> clients;
        private readonly ServiceCollection serviceCollection;
        private readonly ServiceProvider serviceProvider;
        private readonly IElementRepository elementRepository;
        private readonly RootElement root;

        public readonly Configuration configuration;


        public MtaServer(
            string directory, 
            string netDllPath, 
            Configuration? configuration = null,
            Action<ServiceCollection>? dependencyCallback = null
        )
        {
            this.configuration = configuration ?? new Configuration();

            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(this.configuration, new ValidationContext(this.configuration), validationResults, true))
            {
                string invalidProperties = string.Join("\r\n\t", validationResults.Select(r => r.ErrorMessage));
                throw new Exception("An error has occurred while parsing configuration parameters:\r\n " + invalidProperties);
            }

            this.root = new RootElement();

            this.serviceCollection = new ServiceCollection();
            this.SetupDependencies(dependencyCallback);
            this.serviceProvider = this.serviceCollection.BuildServiceProvider();

            this.resourceServer = this.serviceProvider.GetRequiredService<IResourceServer>();
            this.resourceServer.Start();

            this.elementRepository = this.serviceProvider.GetRequiredService<IElementRepository>();

            this.packetReducer = new PacketReducer();
            this.clients = new Dictionary<NetWrapper, Dictionary<uint, Client>>();

            this.netWrapper = CreateNetWrapper(directory, netDllPath, this.configuration.Host, this.configuration.Port);
        }

        public void Start()
        {
            this.netWrapper.Start();
        }

        public void Stop()
        {
            this.netWrapper.Stop();
        }

        public void RegisterPacketQueueHandler(PacketId packetId, IQueueHandler queueHandler)
        {
            this.packetReducer.RegisterQueueHandler(packetId, queueHandler);
        }

        public T Instantiate<T>() => ActivatorUtilities.CreateInstance<T>(this.serviceProvider);
        public T Instantiate<T>(params object[] parameters) 
            => ActivatorUtilities.CreateInstance<T>(this.serviceProvider, parameters);

        public void BroadcastPacket(Packet packet)
        {
            packet.SendTo(this.clients.SelectMany(x => x.Value.Values));
        }

        private void SetupDependencies(Action<ServiceCollection>? dependencyCallback)
        {
            this.serviceCollection.AddSingleton<IElementRepository, CompoundElementRepository>();
            this.serviceCollection.AddSingleton<ILogger, DefaultLogger>();
            this.serviceCollection.AddSingleton<IResourceServer, BasicHttpServer>();
            this.serviceCollection.AddSingleton<Configuration>(this.configuration);
            this.serviceCollection.AddSingleton<RootElement>(this.root);
            this.serviceCollection.AddSingleton<MtaServer>(this);

            dependencyCallback?.Invoke(this.serviceCollection);
        }

        private NetWrapper CreateNetWrapper(string directory, string netDllPath, string host, ushort port)
        {
            NetWrapper netWrapper = new NetWrapper(directory, netDllPath, host, port);
            netWrapper.OnPacketReceived += EnqueueIncomingPacket;

            this.clients[netWrapper] = new Dictionary<uint, Client>();

            return netWrapper;
        }

        private void EnqueueIncomingPacket(NetWrapper netWrapper, uint binaryAddress, PacketId packetId, byte[] data)
        {
            if (!this.clients[netWrapper].ContainsKey(binaryAddress))
            {
                this.clients[netWrapper][binaryAddress] = new Client(binaryAddress, netWrapper);
                OnClientConnect?.Invoke(this.clients[netWrapper][binaryAddress]);
            }

            this.packetReducer.EnqueuePacket(this.clients[netWrapper][binaryAddress], packetId, data);

            if (
                packetId == PacketId.PACKET_ID_PLAYER_QUIT || 
                packetId == PacketId.PACKET_ID_PLAYER_TIMEOUT ||
                packetId == PacketId.PACKET_ID_PLAYER_NO_SOCKET
            )
            {
                this.clients[netWrapper][binaryAddress].IsConnected = false;
                OnClientDisconnect?.Invoke(this.clients[netWrapper][binaryAddress]);
                this.clients[netWrapper].Remove(binaryAddress);
            }
        }

        public event Action<Client>? OnClientConnect;
        public event Action<Client>? OnClientDisconnect;

    }
}
