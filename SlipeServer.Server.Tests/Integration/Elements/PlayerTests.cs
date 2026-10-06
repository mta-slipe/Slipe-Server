using FluentAssertions;
using FluentAssertions.Execution;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Loggers;
using SlipeServer.Server.PacketHandling.Handlers.Player;
using SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;
using SlipeServer.Server.Resources;
using SlipeServer.Server.TestTools;
using System;
using System.Linq;
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

        monitor.OccurredEvents
            .OrderBy(x => x.Sequence)
            .Select(x => x.EventName)
            .Should()
            .Contain(["Kicked", "Disconnected", "Destroyed"]);
    }

    [Fact]
    public void ControlsShouldWork()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();

        player.Controls.ToggleAll(false);
        player.Controls.ForwardsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task StartForAsyncShouldCompleteWhenPlayerAcknowledgesTheResourceStart()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var resource = new Resource(server, server.RootElement, "test");

        var startTask = resource.StartForAsync(player);
        player.TriggerResourceStarted(resource.NetId);

        await startTask;
        startTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task StartForAsyncShouldIgnoreOtherResourcesBeingAcknowledged()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var resource = new Resource(server, server.RootElement, "test");

        var startTask = resource.StartForAsync(player, timeout: TimeSpan.FromSeconds(5));
        player.TriggerResourceStarted((ushort)(resource.NetId + 1));
        startTask.IsCompleted.Should().BeFalse();

        player.TriggerResourceStarted(resource.NetId);
        await startTask;
    }

    [Fact]
    public async Task StartForAsyncShouldThrowWhenCancellationTokenIsAlreadyCancelled()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var resource = new Resource(server, server.RootElement, "test");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => resource.StartForAsync(player, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task StartForAsyncShouldBeCancellable()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var resource = new Resource(server, server.RootElement, "test");

        using var cts = new CancellationTokenSource();
        var startTask = resource.StartForAsync(player, cts.Token);

        await cts.CancelAsync();

        await FluentActions.Awaiting(() => startTask).Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task StartForAsyncShouldStopWhenPlayerDisconnects()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var resource = new Resource(server, server.RootElement, "test");

        var startTask = resource.StartForAsync(player);
        player.TriggerDisconnected(QuitReason.Quit);

        await FluentActions.Awaiting(() => startTask).Should().ThrowAsync<PlayerQuitDuringResourceStartException>();
    }

    [Fact]
    public async Task StartForAsyncShouldStopWhenPlayerIsDestroyed()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var resource = new Resource(server, server.RootElement, "test");

        var startTask = resource.StartForAsync(player);
        player.Destroy();

        await FluentActions.Awaiting(() => startTask).Should().ThrowAsync<PlayerDestroyedDuringResourceStartException>();
    }

    [Fact]
    public async Task StartForAsyncShouldThrowWhenPlayerIsAlreadyDestroyed()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var resource = new Resource(server, server.RootElement, "test");

        player.Destroy();

        var act = () => resource.StartForAsync(player);

        await act.Should().ThrowAsync<PlayerDestroyedDuringResourceStartException>();
    }

    [Fact]
    public async Task StartForAsyncShouldThrowWhenPlayerIsNotConnected()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var resource = new Resource(server, server.RootElement, "test");

        player.Client.IsConnected = false;

        var act = () => resource.StartForAsync(player);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task StartForAsyncShouldTimeOutWhenPlayerNeverAcknowledgesTheResourceStart()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var resource = new Resource(server, server.RootElement, "test");

        var act = () => resource.StartForAsync(player, timeout: TimeSpan.FromMilliseconds(50));

        await act.Should().ThrowAsync<ResourceStartTimeoutException>();
    }

    [Fact]
    public async Task TryStartForAsyncShouldReturnTrueWhenPlayerIsUnavailable()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var resource = new Resource(server, server.RootElement, "test");

        var startTask = resource.StartForAsync(player);
        player.TriggerDisconnected(QuitReason.Quit);

        await FluentActions.Awaiting(() => startTask).Should().ThrowAsync<PlayerUnavailableDuringResourceStartException>();

        var destroyedPlayer = server.AddFakePlayer();
        destroyedPlayer.Destroy();
        (await resource.TryStartForAsync(destroyedPlayer)).Should().BeTrue();
    }
}
