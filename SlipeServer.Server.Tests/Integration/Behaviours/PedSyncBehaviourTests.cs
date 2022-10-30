using FluentAssertions;
using Moq;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Services;
using SlipeServer.Server.TestTools;
using System;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Behaviours;

public class PedSyncBehaviourTests
{
    [Fact]
    public void ClosestPlayerBecomesPedSyncer()
    {
        var server = new TestingServer();

        Action? timerCallback = null;
        var timerServiceMock = new Mock<ITimerService>();
        timerServiceMock
            .Setup(x => x.CreateTimer(It.IsAny<Action>(), It.IsAny<TimeSpan>()))
            .Callback((Action action, TimeSpan timespan) =>
            {
                timerCallback = action;
            });

        var behaviour = new PedSyncBehaviour(
            server,
            server.GetRequiredService<IElementCollection>(),
            server.GetRequiredService<Configuration>(),
            timerServiceMock.Object
        );

        timerCallback.Should().NotBeNull();

        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        player1.Position = new Vector3(10, 0, 0);
        player2.Position = new Vector3(20, 0, 0);

        var ped = new Ped(PedModel.Army, new Vector3(0, 0, 0))
            .AssociateWith(server);

        timerCallback!();

        ped.Syncer.Should().Be(player1);
        player1.SyncingPeds.Should().Contain(ped);
        player2.SyncingPeds.Should().BeEmpty();
    }

    [Fact]
    public void PedDoesNotGetSyncerIfThereIsNoPlayerWithinRange()
    {
        var server = new TestingServer();
        var configuration = server.GetRequiredService<Configuration>();
        configuration.PedSyncerDistance = 100;

        Action? timerCallback = null;
        var timerServiceMock = new Mock<ITimerService>();
        timerServiceMock
            .Setup(x => x.CreateTimer(It.IsAny<Action>(), It.IsAny<TimeSpan>()))
            .Callback((Action action, TimeSpan timespan) =>
            {
                timerCallback = action;
            });

        var behaviour = new PedSyncBehaviour(
            server,
            server.GetRequiredService<IElementCollection>(),
            configuration,
            timerServiceMock.Object
        );

        timerCallback.Should().NotBeNull();

        var player1 = server.AddFakePlayer();

        player1.Position = new Vector3(1000, 0, 0);

        var ped = new Ped(PedModel.Army, new Vector3(0, 0, 0))
            .AssociateWith(server);

        timerCallback!();

        ped.Syncer.Should().BeNull();
        player1.SyncingPeds.Should().BeEmpty();
    }

    [Fact]
    public void PedSyncerAssignedSendsStartSyncPacket()
    {
        var server = new TestingServer();

        Action? timerCallback = null;
        var timerServiceMock = new Mock<ITimerService>();
        timerServiceMock
            .Setup(x => x.CreateTimer(It.IsAny<Action>(), It.IsAny<TimeSpan>()))
            .Callback((Action action, TimeSpan timespan) =>
            {
                timerCallback = action;
            });

        var behaviour = new PedSyncBehaviour(
            server,
            server.GetRequiredService<IElementCollection>(),
            server.GetRequiredService<Configuration>(),
            timerServiceMock.Object
        );

        timerCallback.Should().NotBeNull();

        var player = server.AddFakePlayer();

        player.Position = new Vector3(10, 0, 0);

        var ped = new Ped(PedModel.Army, new Vector3(0, 0, 0))
            .AssociateWith(server);

        timerCallback!();

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_PED_STARTSYNC, player);
    }

    [Fact]
    public void PedSyncerDoesNotSendStartSyncPacketIfAlreadyAssigned()
    {
        var server = new TestingServer();

        Action? timerCallback = null;
        var timerServiceMock = new Mock<ITimerService>();
        timerServiceMock
            .Setup(x => x.CreateTimer(It.IsAny<Action>(), It.IsAny<TimeSpan>()))
            .Callback((Action action, TimeSpan timespan) =>
            {
                timerCallback = action;
            });

        var behaviour = new PedSyncBehaviour(
            server,
            server.GetRequiredService<IElementCollection>(),
            server.GetRequiredService<Configuration>(),
            timerServiceMock.Object
        );

        timerCallback.Should().NotBeNull();

        var player = server.AddFakePlayer();

        player.Position = new Vector3(10, 0, 0);

        var ped = new Ped(PedModel.Army, new Vector3(0, 0, 0))
            .AssociateWith(server);

        ped.Syncer = player;
        player.SyncingPeds.Add(ped);

        timerCallback!();

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_PED_STARTSYNC, player, count: 0);
    }

    [Fact]
    public void PedSyncerUnassignedSendsStopSyncPacket()
    {
        var server = new TestingServer();

        Action? timerCallback = null;
        var timerServiceMock = new Mock<ITimerService>();
        timerServiceMock
            .Setup(x => x.CreateTimer(It.IsAny<Action>(), It.IsAny<TimeSpan>()))
            .Callback((Action action, TimeSpan timespan) =>
            {
                timerCallback = action;
            });

        var behaviour = new PedSyncBehaviour(
            server,
            server.GetRequiredService<IElementCollection>(),
            server.GetRequiredService<Configuration>(),
            timerServiceMock.Object
        );

        timerCallback.Should().NotBeNull();

        var player = server.AddFakePlayer();

        player.Position = new Vector3(1000, 0, 0);

        var ped = new Ped(PedModel.Army, new Vector3(0, 0, 0))
            .AssociateWith(server);

        ped.Syncer = player;
        player.SyncingPeds.Add(ped);

        timerCallback!();

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_PED_STOPSYNC, player);
    }

    [Fact]
    public void PedSyncerSwitchedSendsAppropriatePackets()
    {
        var server = new TestingServer();

        Action? timerCallback = null;
        var timerServiceMock = new Mock<ITimerService>();
        timerServiceMock
            .Setup(x => x.CreateTimer(It.IsAny<Action>(), It.IsAny<TimeSpan>()))
            .Callback((Action action, TimeSpan timespan) =>
            {
                timerCallback = action;
            });

        var behaviour = new PedSyncBehaviour(
            server,
            server.GetRequiredService<IElementCollection>(),
            server.GetRequiredService<Configuration>(),
            timerServiceMock.Object
        );

        timerCallback.Should().NotBeNull();

        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        player1.Position = new Vector3(20, 0, 0);
        player2.Position = new Vector3(10, 0, 0);

        var ped = new Ped(PedModel.Army, new Vector3(0, 0, 0))
            .AssociateWith(server);

        ped.Syncer = player1;
        player1.SyncingPeds.Add(ped);

        timerCallback!();

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_PED_STOPSYNC, player1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_PED_STARTSYNC, player2);
    }
}
