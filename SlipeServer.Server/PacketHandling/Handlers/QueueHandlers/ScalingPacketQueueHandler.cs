using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Rpc;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;

public class ScalingPacketQueueHandler<T> : BasePacketQueueHandler<T> where T : Packet
{
    private readonly QueueHandlerScalingConfig config;
    private readonly int sleepTime;
    private readonly ILogger logger;
    private readonly IPacketHandler<T> packetHandler;
    private readonly Timer timer;
    private readonly Stack<Worker> workers;
    protected TaskCompletionSource<int>? pulseTaskCompletionSource;

    protected struct Worker
    {
        public bool Active { get; set; }
    }

    public ScalingPacketQueueHandler(ILogger logger, IPacketHandler<T> packetHandler, QueueHandlerScalingConfig? config = null, int sleepTime = 10)
    {
        this.logger = logger;
        this.packetHandler = packetHandler;
        this.config = config ?? new();
        this.sleepTime = sleepTime;
        this.workers = new();

        for (int i = 0; i < this.config.MinWorkerCount; i++)
        {
            AddWorker();
        }

        this.timer = new Timer(this.config.NewWorkerTimeout)
        {
            AutoReset = true,
        };
        this.timer.Start();
        this.timer.Elapsed += (sender, args) => CheckWorkerCount();

        this.pulseTaskCompletionSource = new();
    }

    public ScalingPacketQueueHandler(ILogger logger, IPacketHandler<T> packetHandler)
        : this(logger, packetHandler, null)
    {
    }

    public void CheckWorkerCount()
    {
        if (this.packetQueue.Count < this.config.QueueLowThreshold)
        {
            if (this.workers.Count > this.config.MinWorkerCount)
                RemoveWorker();
        } else if (this.packetQueue.Count > this.config.QueueHighThreshold)
        {
            if (this.workers.Count < this.config.MaxWorkerCount)
                AddWorker();
        }
    }

    private void AddWorker()
    {
        var worker = new Worker()
        {
            Active = true
        };
        this.workers.Push(worker);
        Task.Run(() => PulsePacketTask(worker));
    }

    private void RemoveWorker()
    {
        var worker = this.workers.Pop();
        worker.Active = false;
    }

    private async void PulsePacketTask(Worker worker)
    {
        while (worker.Active)
        {
            while (this.packetQueue.TryDequeue(out var queueEntry))
            {
                try
                {
                    this.packetHandler.HandlePacket(queueEntry.Client, queueEntry.Packet);
                    TriggerPacketHandled(queueEntry.Packet);
                }
                catch (Exception e)
                {
                    if (queueEntry.Packet is RpcPacket rpcPacket)
                        this.logger.LogError($"Handling rpcPacket ({rpcPacket.FunctionId}) failed.\n{e.Message}\n{e.StackTrace}");
                    else
                        this.logger.LogError($"Handling packet ({queueEntry.Packet}) failed.\n{e.Message}\n{e.StackTrace}");
                }
            }

            if (this.pulseTaskCompletionSource != null)
            {
                this.pulseTaskCompletionSource.SetResult(0);
                this.pulseTaskCompletionSource = null;
            }

            await Task.Delay(this.sleepTime);
        }
    }

    public Task GetPulseTask()
    {
        this.pulseTaskCompletionSource = new TaskCompletionSource<int>();
        return this.pulseTaskCompletionSource.Task;
    }
}
