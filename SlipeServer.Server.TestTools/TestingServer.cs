using Microsoft.Extensions.DependencyInjection;
using Moq;
using SlipeServer.Net;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.ResourceServing;
using System;

namespace SlipeServer.Server.TestTools
{
    public class TestingServer<TPlayer>: MtaServer where TPlayer: Player
    {
        public Mock<INetWrapper> NetWrapperMock { get; }
        private uint binaryAddressCounter;
        private readonly Func<Client, uint, TPlayer> playerCreationMethod;

        public TestingServer(Func<Client, uint, TPlayer> playerCreationMethod, Configuration configuration = null) : base(configuration, ConfigureOverrides)
        {
            this.playerCreationMethod = playerCreationMethod;
            this.NetWrapperMock = new Mock<INetWrapper>();
            RegisterNetWrapper(this.NetWrapperMock.Object);
        }

        public static void ConfigureOverrides(ServiceCollection services)
        {
            var httpServerMock = new Mock<IResourceServer>();
            services.AddSingleton<IResourceServer>(httpServerMock.Object);
        }

        public TPlayer AddFakePlayer()
        {
            var address = ++this.binaryAddressCounter;
            var client = new Client(address, this.NetWrapperMock.Object);
            var player = this.playerCreationMethod(client, address);
            player.AssociateWith(this);
            return player;
        }

        public void HandlePacket(TestingPlayer source, PacketId packetId, byte[] data)
        {
            this.packetReducer.EnqueuePacket(source.Client, packetId, data);
        }

        public void HandlePacket(uint address, PacketId packetId, byte[] data)
        {
            this.packetReducer.EnqueuePacket(new Client(address, this.NetWrapperMock.Object), packetId, data);
        }

        public uint GenerateBinaryAddress() => ++this.binaryAddressCounter;
    }

    public class TestingServer: TestingServer<TestingPlayer>
    {
        public TestingServer(Configuration configuration = null) : base((client, address) => new TestingPlayer(client, address), configuration)
        {
            
        } 
    }
}
