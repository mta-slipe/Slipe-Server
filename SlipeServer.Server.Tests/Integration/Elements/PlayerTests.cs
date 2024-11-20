﻿using FluentAssertions;
using FluentAssertions.Execution;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Loggers;
using SlipeServer.Server.PacketHandling.Handlers.Player;
using SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;
using SlipeServer.Server.Resources;
using SlipeServer.Server.TestTools;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
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

        server.EnqueuePacketToClient(player1.Client, PacketId.PACKET_ID_PLAYER_WASTED, new byte[] {
            240, 232, 192, 224, 6, 163, 149, 228, 3, 40, 116, 12, 119, 72, 123, 7, 0,
        });

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

    [Fact]
    public async Task StartForAsyncShouldBeCancellable()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();

        var resource = new Resource(server, server.RootElement, "test");

        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        var act = async () => await resource.StartForAsync(player, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task StartForAsyncShouldBeCancellable2()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();

        var resource = new Resource(server, server.RootElement, "test");

        var cts = new CancellationTokenSource();
        
        var act = async () => await resource.StartForAsync(player, cts.Token);

        var waitHandle = new ManualResetEvent(false);
        var cancelled = false;
        var _ = Task.Run(async () =>
        {
            await act.Should().ThrowAsync<TaskCanceledException>();
            waitHandle.Set();
            cancelled = true;
        });

        await Task.Delay(2000); // Let StartForAsync execute for a while to block on "await source.Task"
        await cts.CancelAsync();
        waitHandle.WaitOne(TimeSpan.FromSeconds(5));
        cancelled.Should().BeTrue();
    }

    [Fact]
    public void ResourceShouldStopStartingWhenPlayerDisconnectFromTheServer()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var resource = new Resource(server, server.RootElement, "test");
        var cts = new CancellationTokenSource();
        
        var act = async () => await resource.StartForAsync(player, cts.Token);

        var waitHandle = new ManualResetEvent(false);
        var cancelled = false;
        var _ = Task.Run(async () =>
        {
            await act.Should().ThrowAsync<Exception>();
            waitHandle.Set();
            cancelled = true;
        });

        player.Kick();
        waitHandle.WaitOne(TimeSpan.FromSeconds(5));
        cancelled.Should().BeTrue();
    }
}
