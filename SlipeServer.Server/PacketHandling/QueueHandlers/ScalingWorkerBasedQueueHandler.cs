using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{

    public abstract class ScalingWorkerBasedQueueHandler : BaseQueueHandler
    {
        struct Worker
        {
            public bool Active { get; set; }
        }

        protected abstract Dictionary<PacketId, Type> PacketTypes { get; }
        protected readonly int sleepInterval;
        private readonly int minWorkerCount;
        private readonly int maxWorkerCount;
        private readonly int queueHighThreshold;
        private readonly int queueLowThreshold;
        private readonly Stack<Worker> workers;

        public int WorkerCount => workers.Count;

        private TaskCompletionSource<int>? pulseTaskCompletionSource;

        private readonly Timer timer;

        public ScalingWorkerBasedQueueHandler(
            int sleepInterval = 10, 
            int minWorkerCount = 1, 
            int maxWorkerCount = 10,
            int queueHighThreshold = 20,
            int queueLowThreshold = 3,
            int newWorkerTimeout = 1000
        ) : base()
        {
            this.sleepInterval = sleepInterval;
            this.minWorkerCount = minWorkerCount;
            this.maxWorkerCount = maxWorkerCount;
            this.queueHighThreshold = queueHighThreshold;
            this.queueLowThreshold = queueLowThreshold;
            this.workers = new Stack<Worker>();

            for (int i = 0; i < minWorkerCount; i++)
            {
                AddWorker();
            }

            this.timer = new Timer(newWorkerTimeout)
            {
                AutoReset = true,
            };
            this.timer.Start();
            this.timer.Elapsed += (sender, args) => CheckWorkerCount();
        }

        public ScalingWorkerBasedQueueHandler(QueueHandlerScalingConfig config, int sleepInterval = 10)
            : this(sleepInterval, config.MinWorkerCount, config.MaxWorkerCount, config.QueueHighThreshold, config.QueueLowThreshold, config.NewWorkerTimeout)
        {

        }

        public void CheckWorkerCount()
        {
            if (this.packetQueue.Count < this.queueLowThreshold)
            {
                if (this.workers.Count > this.minWorkerCount)
                    RemoveWorker();
            } else if (this.packetQueue.Count > this.queueHighThreshold)
            {
                if (this.workers.Count < this.maxWorkerCount)
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
            while(worker.Active)
            {
                while (this.packetQueue.TryDequeue(out PacketQueueEntry queueEntry))
                {
                    if (!this.PacketTypes.ContainsKey(queueEntry.PacketId))
                    {
                        throw new NotSupportedException($"Unsupported packet id {queueEntry.PacketId}");
                    }

                    try
                    {
                        var type = this.PacketTypes[queueEntry.PacketId];
                        var packet = Activator.CreateInstance(type) as Packet;
                        if (packet != null)
                        {
                            packet.Read(queueEntry.Data);
                            await HandlePacket(queueEntry.Client, packet);
                        }
                    } catch (Exception e)
                    {
                        if (!this.PacketTypes.ContainsKey(queueEntry.PacketId))
                        {
                            throw new NotSupportedException($"Error handling packet id {queueEntry.PacketId}", e);
                        }
                    }
                }

                if (this.pulseTaskCompletionSource != null)
                {
                    this.pulseTaskCompletionSource.SetResult(0);
                    this.pulseTaskCompletionSource = null;
                }

                await Task.Delay(this.sleepInterval);
            }
        }

        protected abstract Task HandlePacket(Client client, Packet packet);

        public Task GetPulseTask()
        {
            this.pulseTaskCompletionSource = new TaskCompletionSource<int>();
            return this.pulseTaskCompletionSource.Task;
        }
    }
}
