using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.ServerBuilders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.TestTools;

public class TestingServer<TPlayer> : MtaServer<TPlayer> 
    where TPlayer : TestingPlayer, new()
{
    public Mock<INetWrapper> NetWrapperMock { get; }
    private uint binaryAddressCounter;

    private readonly List<SendPacketCall> sendPacketCalls;

    public TestingServer(Configuration configuration = null, Action<ServerBuilder> configure = null) : base(x =>
    {
        x.UseConfiguration(configuration ?? new());
        x.ConfigureServices(ConfigureOverrides);
        configure?.Invoke(x);
    })
    {
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
        var client = CreateClient(address, this.NetWrapperMock.Object);
        var player = client.Player as TPlayer;

        this.clients[this.NetWrapperMock.Object].Add(address, client);

        player.AssociateWith(this);

        this.HandlePlayerJoin(player);

        return player;
    }

    public void HandlePacket(TestingPlayer source, PacketId packetId, byte[] data)
    {
        this.packetReducer.EnqueuePacket(source.Client, packetId, data);
    }

    public void HandlePacket(uint address, PacketId packetId, byte[] data)
    {
        var sourceClient = CreateClient(0, this.NetWrapperMock.Object);
        this.packetReducer.EnqueuePacket(sourceClient, packetId, data);
    }

    protected override IClient CreateClient(uint binaryAddress, INetWrapper netWrapper)
    {
        var player = new TestingPlayer();
        player.Client = new TestingClient(binaryAddress, netWrapper, player);
        return player.Client;
    }

    public void VerifyPacketSent(PacketId packetId, TPlayer to, byte[] data = null, int count = 1)
    {
        this.sendPacketCalls.Count(x =>
            x.PacketId == packetId && x.Address == to.Address && (data == null || x.Data.SequenceEqual(data))
        ).Should().Be(count);
    }

    public void VerifyLuaElementRpcPacketSent(ElementRpcFunction packetId, TPlayer to, byte[] data = null, int count = 1)
    {
        this.sendPacketCalls.Count(x =>
            x.PacketId == PacketId.PACKET_ID_LUA_ELEMENT_RPC && 
            x.Address == to.Address && 
            x.Data[0] == (byte)packetId 
            && (data == null || x.Data.SequenceEqual(data))
        ).Should().Be(count);
    }

    public void ResetPacketCountVerification() => this.sendPacketCalls.Clear();

    public uint GenerateBinaryAddress() => ++this.binaryAddressCounter;
}

public class TestingServer : TestingServer<TestingPlayer>
{
    public TestingServer(Configuration configuration = null, Action<ServerBuilder>? configure = null) : base(configuration, configure)
    {

    }
}
