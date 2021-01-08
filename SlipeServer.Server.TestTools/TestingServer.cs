using Microsoft.Extensions.DependencyInjection;
using Moq;
using SlipeServer.Net;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Resources.ResourceServing;
using System;

namespace SlipeServer.Server.TestTools
{
    public class TestingServer: MtaServer
    {
        public Mock<INetWrapper> NetWrapperMock { get; }
        private uint binaryAddressCounter;

        public TestingServer(Configuration configuration = null
        ) : base(configuration, ConfigureOverrides)
        {
            this.NetWrapperMock = new Mock<INetWrapper>();
            RegisterNetWrapper(NetWrapperMock.Object);
        }

        public static void ConfigureOverrides(ServiceCollection services)
        {
            var httpServerMock = new Mock<IResourceServer>();
            services.AddSingleton<IResourceServer>(httpServerMock.Object);
        }

        public TestingPlayer AddFakePlayer()
        {
            var address = ++binaryAddressCounter;
            var client = new Client(address, this.NetWrapperMock.Object);
            return new TestingPlayer(client, address).AssociateWith(this);
        }

        public void HandlePacket(TestingPlayer source, PacketId packetId, byte[] data)
        {
            this.packetReducer.EnqueuePacket(source.Client, packetId, data);
        }

        public void HandlePacket(uint address, PacketId packetId, byte[] data)
        {
            this.packetReducer.EnqueuePacket(new Client(address, this.NetWrapperMock.Object), packetId, data);
        }

        public uint GenerateBinaryAddress() => ++binaryAddressCounter;
    }
}
