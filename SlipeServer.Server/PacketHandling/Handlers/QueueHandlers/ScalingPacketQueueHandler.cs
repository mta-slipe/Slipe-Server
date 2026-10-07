using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Rpc;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;

public class ScalingPacketQueueHandler<T> : BasePacketQueueHandler<T> where T : Packet
{
    private readonly QueueHandlerScalingConfig config;
    private readonly int sleepTime;
    private readonly ILogger logger;
    private readonly IPacketHandler<T> packetHandler;
    private readonly Timer timer;
    private readonly Stack<Worker> workers = [];
    private readonly Lock workersLock = new();
    private readonly CancellationTokenSource stopCancellationTokenSource = new();
    private TaskCompletionSource<int>? pulseTaskCompletionSource;
    private int activeWorkerCount;
    private bool disposed;

    /// <summary>
    /// The number of worker loops that are currently running. This can briefly differ from the
    /// configured worker count, since workers are started asynchronously and stop after their
    /// current iteration.
    /// </summary>
    public int ActiveWorkerCount => Volatile.Read(ref this.activeWorkerCount);

    private sealed class Worker
    {
        public volatile bool Active = true;
        public Task LoopTask = Task.CompletedTask;
    }

    public ScalingPacketQueueHandler(ILogger logger, IPacketHandler<T> packetHandler, QueueHandlerScalingConfig? config = null, int sleepTime = 10)
    {
        if (sleepTime < 1)
            throw new ArgumentOutOfRangeException(nameof(sleepTime), sleepTime, "Sleep time must be at least 1 millisecond.");

        this.logger = logger;
        this.packetHandler = packetHandler;
        this.config = config ?? new();
        this.sleepTime = sleepTime;

        this.config.Validate();

        for (int i = 0; i < this.config.MinWorkerCount; i++)
        {
            AddWorker();
        }

        this.timer = new Timer(this.config.NewWorkerTimeout)
        {
            AutoReset = true,
        };
        this.timer.Elapsed += (sender, args) => CheckWorkerCount();
        this.timer.Start();
    }

    public ScalingPacketQueueHandler(ILogger logger, IPacketHandler<T> packetHandler)
        : this(logger, packetHandler, null)
    {
    }

    public void CheckWorkerCount()
    {
        if (this.disposed)
            return;

        var queueCount = this.packetQueue.Count;
        lock (this.workersLock)
        {
            if (queueCount < this.config.QueueLowThreshold)
            {
                if (this.workers.Count > this.config.MinWorkerCount)
                    RemoveWorker();
            }
            else if (queueCount > this.config.QueueHighThreshold)
            {
                if (this.workers.Count < this.config.MaxWorkerCount)
                    AddWorker();
            }
        }
    }

    private void AddWorker()
    {
        lock (this.workersLock)
        {
            var worker = new Worker();
            this.workers.Push(worker);
            worker.LoopTask = Task.Run(() => PulsePacketTask(worker));
        }
    }

    private void RemoveWorker()
    {
        lock (this.workersLock)
        {
            if (this.workers.TryPop(out var worker))
                worker.Active = false;
        }
    }

    private async Task PulsePacketTask(Worker worker)
    {
        using var ticker = new PeriodicTimer(TimeSpan.FromMilliseconds(this.sleepTime));
        Interlocked.Increment(ref this.activeWorkerCount);
        try
        {
            while (worker.Active)
            {
                while (this.packetQueue.TryDequeue(out var queueEntry))
                {
                    try
                    {
                        ClientContext.Current = queueEntry.Client;
                        this.packetHandler.HandlePacket(queueEntry.Client, queueEntry.Packet);
                        TriggerPacketHandled(queueEntry.Packet);
                    }
                    catch (Exception e)
                    {
                        if (queueEntry.Packet is RpcPacket rpcPacket)
                            this.logger.LogError(e, "Handling rpc packet ({FunctionId}) failed.", rpcPacket.FunctionId);
                        else
                            this.logger.LogError(e, "Handling packet ({Packet}) failed.", queueEntry.Packet);
                    }
                    finally
                    {
                        ClientContext.Current = null;
                    }
                }

                var pulse = Interlocked.Exchange(ref this.pulseTaskCompletionSource, null);
                pulse?.TrySetResult(0);

                if (!await ticker.WaitForNextTickAsync(this.stopCancellationTokenSource.Token))
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation is the normal shutdown path.
        }
        finally
        {
            Interlocked.Decrement(ref this.activeWorkerCount);
        }
    }

    public Task GetPulseTask()
    {
        var pulse = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        Interlocked.Exchange(ref this.pulseTaskCompletionSource, pulse);
        return pulse.Task;
    }

    public override void Dispose()
    {
        if (this.disposed)
            return;
        this.disposed = true;

        this.timer.Stop();
        this.timer.Dispose();

        Worker[] remainingWorkers;
        lock (this.workersLock)
        {
            remainingWorkers = this.workers.ToArray();
            this.workers.Clear();
        }

        foreach (var worker in remainingWorkers)
            worker.Active = false;

        this.stopCancellationTokenSource.Cancel();

        var workerTasks = new Task[remainingWorkers.Length];
        for (int i = 0; i < remainingWorkers.Length; i++)
            workerTasks[i] = remainingWorkers[i].LoopTask;

        try
        {
            if (!Task.WaitAll(workerTasks, this.config.WorkerShutdownTimeout))
                this.logger.LogWarning("Timed out waiting for {WorkerCount} packet queue worker(s) to stop.", workerTasks.Length);
        }
        catch (AggregateException e)
        {
            this.logger.LogError(e, "Packet queue worker(s) faulted while stopping.");
        }

        this.stopCancellationTokenSource.Dispose();
        base.Dispose();
    }
}
