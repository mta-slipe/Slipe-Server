﻿using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.ServerBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SlipeServer.Server.TestTools;

public class TestingServer<TPlayer> : MtaServer<TPlayer> 
    where TPlayer : Player
{
    public Mock<INetWrapper> NetWrapperMock { get; }
    private uint binaryAddressCounter;

    private readonly List<SendPacketCall> sendPacketCalls;
    public event Action Stopped;

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
    
    public TestingServer(IServiceProvider serviceProvider, Action<ServerBuilder> builderAction) : base(serviceProvider, builderAction)
    {
        this.NetWrapperMock = new Mock<INetWrapper>();
        this.clients[this.NetWrapperMock.Object] = new();
        this.sendPacketCalls = new();
        RegisterNetWrapper(this.NetWrapperMock.Object);
        SetupSendPacketMocks();
    }

    public IEnumerable<SendLuaEvent> GetSendLuaEvents()
    {
        foreach (var sendPacketCall in this.sendPacketCalls)
        {
            if(sendPacketCall.PacketId == PacketId.PACKET_ID_LUA_EVENT)
            {
                var luaEventPacket = new LuaEventPacket();
                luaEventPacket.Read(sendPacketCall.Data);
                yield return new SendLuaEvent
                {
                    Address = sendPacketCall.Address,
                    Name = luaEventPacket.Name,
                    Arguments = luaEventPacket.LuaValues.ToArray(),
                    Source = luaEventPacket.ElementId
                };
            }
        }
    }

    private void SetupSendPacketMocks()
    {
        this.NetWrapperMock.Setup(x => x.SendPacket(It.IsAny<ulong>(), It.IsAny<ushort>(), It.IsAny<Packet>()))
            .Callback((ulong address, ushort version, Packet packet) =>
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
            It.IsAny<ulong>(),
            It.IsAny<PacketId>(), 
            It.IsAny<ushort>(), 
            It.IsAny<byte[]>(), 
            It.IsAny<PacketPriority>(), 
            It.IsAny<PacketReliability>()
        )).Callback((ulong address, PacketId packetId, ushort version, byte[] data, PacketPriority priority, PacketReliability reliability) =>
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

    public static void ConfigureOverrides(IServiceCollection services)
    {
        var httpServerMock = new Mock<IResourceServer>();
        services.AddSingleton<IResourceServer>(httpServerMock.Object);
        services.AddLogging();
        services.AddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
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

    protected override IClient CreateClient(ulong binaryAddress, INetWrapper netWrapper)
    {
        Player player;
        if (typeof(TPlayer) == typeof(Player) || typeof(TPlayer) == typeof(TestingPlayer))
        {
            player = new TestingPlayer();
        }
        else
        {
            player = Instantiate<TPlayer>();
        }
        player.Client = new TestingClient(binaryAddress, netWrapper, player);
        return player.Client;
    }

    public void VerifyPacketSent(PacketId packetId, TPlayer to, byte[] data = null, int count = 1)
    {
        this.sendPacketCalls.Count(x =>
            x.PacketId == packetId && x.Address == to.GetAddress() && (data == null || x.Data.SequenceEqual(data))
        ).Should().Be(count);
    }

    public void VerifyLuaElementRpcPacketSent(ElementRpcFunction packetId, TPlayer to, byte[] data = null, int count = 1)
    {
        this.sendPacketCalls.Count(x =>
            x.PacketId == PacketId.PACKET_ID_LUA_ELEMENT_RPC && 
            x.Address == to.GetAddress() && 
            x.Data[0] == (byte)packetId 
            && (data == null || x.Data.SequenceEqual(data))
        ).Should().Be(count);
    }

    public void VerifyLuaEventTriggered(string eventName, TPlayer to, Element source, LuaValue[] luaValues, int expectedCount = 1)
    {
        var luaEventPacket = new LuaEventPacket();
        int count = 0;
        foreach (var sendPacketCall in this.sendPacketCalls)
        {
            if (sendPacketCall.PacketId == PacketId.PACKET_ID_LUA_EVENT)
            {
                luaEventPacket.Read(sendPacketCall.Data);
                if(luaEventPacket.Name == eventName && luaEventPacket.ElementId == source.Id && sendPacketCall.Address == to.GetAddress())
                {
                    var packetLuaValues = luaEventPacket.LuaValues.ToArray();
                    if (packetLuaValues.Length != luaValues.Length)
                        break;

                    for(int i = 0; i < luaValues.Length; i++)
                    {
                        if (packetLuaValues[i] != luaValues[i])
                            break;
                    }

                    count++;
                }
            }
        }

        count.Should().Be(expectedCount);
    }

    public void ResetPacketCountVerification() => this.sendPacketCalls.Clear();

    public uint GenerateBinaryAddress() => ++this.binaryAddressCounter;


    /// <summary>
    /// Starts the networking interfaces, allowing clients to connect and packets to be sent out to clients.
    /// </summary>
    public override void Start()
    {
        this.StartDatetime = DateTime.Now;

        this.resourceProvider?.Refresh();

        foreach (var server in this.resourceServers)
            server.Start();

        this.IsRunning = true;
    }

    public override void Stop()
    {
        Stopped?.Invoke();

        base.Stop();
    }
}

public class TestingServer : TestingServer<TestingPlayer>
{
    public TestingServer(Configuration configuration = null, Action<ServerBuilder>? configure = null) : base(configuration, configure)
    {

    }

    public TestingServer(IServiceProvider serviceProvider, Action<ServerBuilder> builderAction) : base(serviceProvider, builderAction)
    {

    }
}
