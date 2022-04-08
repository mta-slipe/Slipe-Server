using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Resources.Serving;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.TestTools;

public class TestingServer<TPlayer> : MtaServer where TPlayer : TestingPlayer
{
    public Mock<INetWrapper> NetWrapperMock { get; }
    private uint binaryAddressCounter;
    private readonly Func<Client, uint, TPlayer> playerCreationMethod;

    private readonly List<SendPacketCall> sendPacketCalls;

    public TestingServer(Func<Client, uint, TPlayer> playerCreationMethod, Configuration configuration = null) : base(configuration, ConfigureOverrides)
    {
        this.playerCreationMethod = playerCreationMethod;
        this.NetWrapperMock = new Mock<INetWrapper>();
        this.clients[this.NetWrapperMock.Object] = new();
        this.sendPacketCalls = new();
        RegisterNetWrapper(this.NetWrapperMock.Object);
        SetupSendPacketMocks();
    }

    private void SetupSendPacketMocks()
    {
        this.NetWrapperMock.Setup(x => x.SendPacket(It.IsAny<uint>(), It.IsAny<ushort>(), It.IsAny<Packet>()))
            .Callback((uint address, ushort version, Packet packet) =>
            {
                this.sendPacketCalls.Add(new SendPacketCall()
                {
                    Address = address,
                    Version = version,
                    PacketId = packet.PacketId,
                    Data = packet.Write(),
                    Priority = packet.Priority,
                    Reliability = packet.Reliability
                });
            });

        this.NetWrapperMock.Setup(x => x.SendPacket(
            It.IsAny<uint>(),
            It.IsAny<PacketId>(), 
            It.IsAny<ushort>(), 
            It.IsAny<byte[]>(), 
            It.IsAny<PacketPriority>(), 
            It.IsAny<PacketReliability>()
        )).Callback((uint address, PacketId packetId, ushort version, byte[] data, PacketPriority priority, PacketReliability reliability) =>
            {
                this.sendPacketCalls.Add(new SendPacketCall()
                {
                    Address = address,
                    Version = version,
                    PacketId = packetId,
                    Data = data,
                    Priority = priority,
                    Reliability = reliability
                });
            });
    }

    public static void ConfigureOverrides(ServiceCollection services)
    {
        var httpServerMock = new Mock<IResourceServer>();
        services.AddSingleton<IResourceServer>(httpServerMock.Object);
    }

    public TPlayer AddFakePlayer()
    {
        var address = ++this.binaryAddressCounter;
        var client = new TestingClient(address, this.NetWrapperMock.Object);
        var player = this.playerCreationMethod(client, address);

        this.clients[this.NetWrapperMock.Object].Add(address, client);

        client.TestingPlayer = player;
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

    public void VerifyPacketSent(PacketId packetId, TPlayer to, byte[] data = null, int count = 1)
    {
        this.sendPacketCalls.Count(x =>
            x.PacketId == packetId && x.Address == to.Address && (data == null || x.Data.SequenceEqual(data))
        ).Should().Be(count);
    }

    public uint GenerateBinaryAddress() => ++this.binaryAddressCounter;
}

public class TestingServer : TestingServer<TestingPlayer>
{
    public TestingServer(Configuration configuration = null) : base((client, address) => new TestingPlayer(client, address), configuration)
    {

    }
}
