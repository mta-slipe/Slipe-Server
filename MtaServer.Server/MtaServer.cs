using MtaServer.ConfigurationProviders;
using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using MtaServer.Server.PacketHandling;
using MtaServer.Server.Repositories;
using MTAServerWrapper.Server;
using System.Collections.Generic;

namespace MtaServer.Server
{
    public class MtaServer
    {
        private readonly NetWrapper netWrapper;
        private readonly PacketReducer packetReducer;
        private readonly Dictionary<NetWrapper, Dictionary<uint, Client>> clients;

        public Element Root { get; }
        public ServerConfiguration Configuration { get; }
        public IElementRepository ElementRepository { get; private set; }

        public MtaServer(string directory, string netDllPath, IElementRepository elementRepository, Configuration configuration = null)
        {
            this.ElementRepository = elementRepository;

            if (configuration == null)
                this.Configuration = new ServerConfiguration();
            else
                this.Configuration = new ServerConfiguration(configuration);

            string property;
            if(!this.Configuration.Verify(out property))
                throw new System.Exception($"Property {property} has invalid value.");

            this.Root = new Element();

            this.packetReducer = new PacketReducer();
            this.clients = new Dictionary<NetWrapper, Dictionary<uint, Client>>();

            this.netWrapper = CreateNetWrapper(directory, netDllPath, Configuration.GetHost(), Configuration.GetPort());
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
            }
            this.packetReducer.EnqueuePacket(this.clients[netWrapper][binaryAddress], packetId, data);
        }

    }
}
