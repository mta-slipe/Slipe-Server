using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using System;
using System.Threading.Tasks;

namespace SlipeServer.Server.PacketHandling.Handlers.QueueHandlers
{
    public class WorkerBasedPacketQueueHandler<T> : BasePacketQueueHandler<T> where T : Packet
    {
        private readonly int workerCount;
        private readonly int sleepTime;
        private readonly ILogger logger;
        private readonly IPacketHandler<T> packetHandler;
        private TaskCompletionSource<int>? pulseTaskCompletionSource;

        public WorkerBasedPacketQueueHandler(ILogger logger, IPacketHandler<T> packetHandler, int workerCount = 1, int sleepTime = 10)
        {
            this.logger = logger;
            this.packetHandler = packetHandler;
            this.workerCount = workerCount;
            this.sleepTime = sleepTime;

            for (int i = 0; i < this.workerCount; i++)
            {
                Task.Run(PulsePacketTask);
            }
        }

        private async void PulsePacketTask()
        {
            while (true)
            {
                while (this.packetQueue.TryDequeue(out var queueEntry))
                {
                    try
                    {
                        this.packetHandler.HandlePacket(queueEntry.Client, queueEntry.Packet);
                        TriggerPacketHandled(queueEntry.Packet);
                    } catch (Exception e)
                    {
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
}
