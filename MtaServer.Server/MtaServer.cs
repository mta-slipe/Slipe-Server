using MtaServer.Packets.Enums;
﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MtaServer.Net;
using MtaServer.Packets;
using MtaServer.Server.Elements;
using MtaServer.Server.Extensions;
using MtaServer.Server.PacketHandling;
using MtaServer.Server.Repositories;
using MtaServer.Server.ResourceServing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using MtaServer.Server.AllSeeingEye;
using MtaServer.Server.Elements.IdGeneration;
using MtaServer.Server.Events;

namespace MtaServer.Server
{
    public enum VersionType
    {
        Custom = 0x01,
        Experimental = 0x03,
        Unstable = 0x05,
        Untested = 0x07,
        Release = 0x09,
    }

    public class MtaServer
    {
        private readonly NetWrapper netWrapper;
        private readonly IResourceServer resourceServer;
        private readonly PacketReducer packetReducer;
        private readonly Dictionary<NetWrapper, Dictionary<uint, Client>> clients;
        private readonly ServiceCollection serviceCollection;
        private readonly ServiceProvider serviceProvider;
        private readonly IElementRepository elementRepository;
        private readonly IElementIdGenerator? elementIdGenerator;
        private readonly RootElement root;
        public readonly Configuration configuration;

        public string GameType { get; set; } = "unknown";
        public string MapName { get; set; } = "unknown";
        public string Password { get; set; } = "";
        public bool HasPassword { get => (Password != ""); }

        public DateTime StartDatetime { get; } = DateTime.Now;
        public long Uptime { get => DateTime.Now.Ticks - StartDatetime.Ticks; }


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
            this.elementIdGenerator = this.serviceProvider.GetService<IElementIdGenerator>();

            this.elementRepository.Add(this.root);

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

        public void HandleLuaEvent(LuaEvent luaEvent) => this.LuaEventTriggered?.Invoke(luaEvent);

        private void SetupDependencies(Action<ServiceCollection>? dependencyCallback)
        {
            this.serviceCollection.AddSingleton<IElementRepository, CompoundElementRepository>();
            this.serviceCollection.AddSingleton<ILogger, DefaultLogger>();
            this.serviceCollection.AddSingleton<IResourceServer, BasicHttpServer>();
            this.serviceCollection.AddSingleton<IElementIdGenerator, ElementIdGenerator>();
            this.serviceCollection.AddSingleton<IAseQueryService, AseQueryService>();
            this.serviceCollection.AddSingleton<HttpClient>(new HttpClient());
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
                var client = new Client(binaryAddress, netWrapper);
                AssociateElement(client.Player);

                this.clients[netWrapper][binaryAddress ]= client;
                ClientConnected?.Invoke(client);
            }

            this.packetReducer.EnqueuePacket(this.clients[netWrapper][binaryAddress], packetId, data);

            if (
                packetId == PacketId.PACKET_ID_PLAYER_QUIT || 
                packetId == PacketId.PACKET_ID_PLAYER_TIMEOUT ||
                packetId == PacketId.PACKET_ID_PLAYER_NO_SOCKET
            )
            {
                this.clients[netWrapper][binaryAddress].IsConnected = false;
                ClientDisconnected?.Invoke(this.clients[netWrapper][binaryAddress]);
                this.clients[netWrapper].Remove(binaryAddress);
            }
        }

        public T AssociateElement<T>(T element) where T : Element
        {
            return HandleElementCreation(element);
        }

        private T HandleElementCreation<T>(T element) where T : Element
        {
            if (this.elementIdGenerator != null)
            {
                element.Id = this.elementIdGenerator.GetId();
            }
            this.elementRepository.Add(element);

            this.ElementCreated?.Invoke(element);

            return element;
        }

        public void HandlePlayerJoin(Player player)
        {
            PlayerJoined?.Invoke(player);
        }

        public event Action<Element>? ElementCreated;
        public event Action<Player>? PlayerJoined;
        public event Action<Client>? ClientConnected;
        public event Action<Client>? ClientDisconnected;
        public event Action<LuaEvent>? LuaEventTriggered;

    }
}
