﻿using FluentAssertions;
using FluentAssertions.Execution;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Loggers;
using SlipeServer.Server.PacketHandling.Handlers.Player;
using SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;
using SlipeServer.Server.TestTools;
using System.Linq;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Elements;
public class PlayerTests
{
    [Fact]
    public async void WastedPacketReceived_Relays_wasted_packet()
    {
        var server = new TestingServer();
        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        var handler = server.Instantiate<PlayerWastedPacketHandler>();
        var queueHandler = new ScalingPacketQueueHandler<PlayerWastedPacket>(new NullLogger(), handler);
        server.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_WASTED, queueHandler);

        server.EnqueuePacketToClient(player1.Client, PacketId.PACKET_ID_PLAYER_WASTED, [
            240, 232, 192, 224, 6, 163, 149, 228, 3, 40, 116, 12, 119, 72, 123, 7, 0,
        ]);

        await queueHandler.GetPulseTask();

        server.VerifyPacketSent(PacketId.PACKET_ID_PLAYER_WASTED, player1, count: 1);
        server.VerifyPacketSent(PacketId.PACKET_ID_PLAYER_WASTED, player2, count: 1);
    }

    [Fact]
    public void KillMethod_Relays_wasted_packet()
    {
        var server = new TestingServer();
        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        player1.Kill();

        server.VerifyPacketSent(PacketId.PACKET_ID_PLAYER_WASTED, player1, count: 1);
        server.VerifyPacketSent(PacketId.PACKET_ID_PLAYER_WASTED, player2, count: 1);
    }

    [Fact]
    public void KickingPlayerShouldDestroyAndDisconnectPlayer()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();

        using var monitor = player.Monitor();

        player.Kick();

        using var _ = new AssertionScope();

        player.IsDestroyed.Should().BeTrue();
        player.Client.IsConnected.Should().BeFalse();

        monitor.OccurredEvents.Should().HaveCount(3);
        monitor.OccurredEvents
            .OrderBy(x => x.Sequence)
            .Select(x => x.EventName)
            .Should().BeEquivalentTo(["Kicked", "Disconnected", "Destroyed"]);
    }
}
