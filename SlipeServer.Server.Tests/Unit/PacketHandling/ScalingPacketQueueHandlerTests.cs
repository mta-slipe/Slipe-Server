using FluentAssertions;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Loggers;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.PacketHandling;

public class ScalingPacketQueueHandlerTests
{
    private static QueueHandlerScalingConfig CreateConfig() => new()
    {
        MinWorkerCount = 1,
        MaxWorkerCount = 2,
        QueueLowThreshold = 1,
        QueueHighThreshold = 2,
        // Keep the scaling timer out of the way so tests drive scaling explicitly.
        NewWorkerTimeout = 100000,
        WorkerShutdownTimeout = 5000,
    };

    private static BlockingPacketHandler CreateHandler(ManualResetEventSlim gate, ManualResetEventSlim? handling = null) =>
        new(gate, handling ?? new ManualResetEventSlim(false));

    private static async Task WaitForAsync(Func<bool> condition)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!condition())
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
                throw new TimeoutException("Condition was not met within the timeout.");

            await Task.Delay(10);
        }
    }

    [Fact]
    public async Task Handler_scales_up_when_the_queue_grows()
    {
        var gate = new ManualResetEventSlim(false);
        using var handler = new ScalingPacketQueueHandler<PlayerWastedPacket>(
            new NullLogger(), CreateHandler(gate), CreateConfig());

        await WaitForAsync(() => handler.ActiveWorkerCount == 1);

        for (int i = 0; i < 10; i++)
            handler.EnqueuePacket(null!, new PlayerWastedPacket());

        handler.CheckWorkerCount();

        await WaitForAsync(() => handler.ActiveWorkerCount == 2);

        gate.Set();
    }

    [Fact]
    public async Task Handler_scales_down_and_stops_the_removed_worker_when_the_queue_drains()
    {
        var gate = new ManualResetEventSlim(false);
        using var handler = new ScalingPacketQueueHandler<PlayerWastedPacket>(
            new NullLogger(), CreateHandler(gate), CreateConfig());

        await WaitForAsync(() => handler.ActiveWorkerCount == 1);

        for (int i = 0; i < 10; i++)
            handler.EnqueuePacket(null!, new PlayerWastedPacket());

        handler.CheckWorkerCount();
        await WaitForAsync(() => handler.ActiveWorkerCount == 2);

        gate.Set();
        await WaitForAsync(() => handler.QueuedPacketCount == 0);

        handler.CheckWorkerCount();

        // The removed worker must actually stop running, not just be dropped from the pool.
        await WaitForAsync(() => handler.ActiveWorkerCount == 1);
    }

    [Fact]
    public async Task Dispose_stops_all_workers()
    {
        using var gate = new ManualResetEventSlim(true);
        var handler = new ScalingPacketQueueHandler<PlayerWastedPacket>(
            new NullLogger(), CreateHandler(gate), CreateConfig());

        await WaitForAsync(() => handler.ActiveWorkerCount == 1);

        handler.Dispose();

        handler.ActiveWorkerCount.Should().Be(0);
    }

    [Fact]
    public async Task Dispose_waits_for_packets_that_are_already_being_handled()
    {
        var gate = new ManualResetEventSlim(false);
        var handling = new ManualResetEventSlim(false);
        var handler = new ScalingPacketQueueHandler<PlayerWastedPacket>(
            new NullLogger(), CreateHandler(gate, handling), CreateConfig());

        handler.EnqueuePacket(null!, new PlayerWastedPacket());
        await WaitForAsync(() => handling.IsSet);

        var disposeTask = Task.Run(handler.Dispose);
        await Task.Delay(100);
        disposeTask.IsCompleted.Should().BeFalse("the in-flight packet is still being handled");

        gate.Set();
        await disposeTask.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Dispose_is_idempotent()
    {
        using var gate = new ManualResetEventSlim(true);
        var handler = new ScalingPacketQueueHandler<PlayerWastedPacket>(
            new NullLogger(), CreateHandler(gate), CreateConfig());

        handler.Dispose();
        handler.Invoking(x => x.Dispose()).Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_rejects_invalid_sleep_time(int sleepTime)
    {
        using var gate = new ManualResetEventSlim(true);
        Action act = () => new ScalingPacketQueueHandler<PlayerWastedPacket>(
            new NullLogger(), CreateHandler(gate), CreateConfig(), sleepTime);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_rejects_invalid_config()
    {
        using var gate = new ManualResetEventSlim(true);
        var config = CreateConfig();
        config.MaxWorkerCount = 0;

        Action act = () => new ScalingPacketQueueHandler<PlayerWastedPacket>(
            new NullLogger(), CreateHandler(gate), config);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private sealed class BlockingPacketHandler : IPacketHandler<PlayerWastedPacket>
    {
        private readonly ManualResetEventSlim gate;
        private readonly ManualResetEventSlim handling;

        public BlockingPacketHandler(ManualResetEventSlim gate, ManualResetEventSlim handling)
        {
            this.gate = gate;
            this.handling = handling;
        }

        public PacketId PacketId => PacketId.PACKET_ID_PLAYER_WASTED;

        public void HandlePacket(IClient client, PlayerWastedPacket packet)
        {
            this.handling.Set();
            this.gate.Wait();
        }
    }
}
