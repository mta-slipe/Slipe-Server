using MtaServer.Server.ASE;
using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using MtaServer.Server.PacketHandling;
using MtaServer.Server.Repositories;
using MTAServerWrapper.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
        private readonly PacketReducer packetReducer;
        private readonly Dictionary<NetWrapper, Dictionary<uint, Client>> clients;

        public Element Root { get; }
        public ASE.ASE Ase { get; }
        public Configuration Configuration { get; }
        public IElementRepository ElementRepository { get; private set; }
        public string GameType { get; set; } = "unknown";
        public string MapName { get; set; } = "unknown";

        public MtaServer(string directory, string netDllPath, IElementRepository elementRepository, Configuration? configuration = null)
        {
            this.ElementRepository = elementRepository;

            this.Configuration = configuration ?? new Configuration();

            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(Configuration, new ValidationContext(Configuration), validationResults, true))
            {
                string invalidProperties = string.Join("\r\n\t",validationResults.Select(r => r.ErrorMessage));
                throw new System.Exception("An error has occurred while parsing configuration parameters:\r\n " + invalidProperties);
            }

            this.Root = new Element();

            this.packetReducer = new PacketReducer();
            this.clients = new Dictionary<NetWrapper, Dictionary<uint, Client>>();

            this.netWrapper = CreateNetWrapper(directory, netDllPath, Configuration.Host, Configuration.Port);

            this.Ase = new ASE.ASE(this);
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

            if (packetId == PacketId.PACKET_ID_PLAYER_QUIT || packetId == PacketId.PACKET_ID_PLAYER_TIMEOUT)
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
