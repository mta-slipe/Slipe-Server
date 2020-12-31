using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public abstract class WorkerBasedQueueHandler : BaseQueueHandler
    {
        protected readonly int sleepInterval;
        private TaskCompletionSource<int>? pulseTaskCompletionSource;

        public WorkerBasedQueueHandler(int sleepInterval = 10, int workerCount = 1) : base()
        {
            this.sleepInterval = sleepInterval;

            for (int i = 0; i < workerCount; i++)
            {
                Task.Run(PulsePacketTask);
            }
        }

        private async void PulsePacketTask()
        {
            while(true)
            {
                while (this.packetQueue.TryDequeue(out PacketQueueEntry queueEntry))
                {
                    HandlePacket(queueEntry);
                }

                if (this.pulseTaskCompletionSource != null)
                {
                    this.pulseTaskCompletionSource.SetResult(0);
                    this.pulseTaskCompletionSource = null;
                }

                await Task.Delay(this.sleepInterval);
            }
        }

        protected abstract void HandlePacket(PacketQueueEntry queueEntry);

        public Task GetPulseTask()
        {
            this.pulseTaskCompletionSource = new TaskCompletionSource<int>();
            return this.pulseTaskCompletionSource.Task;
        }
    }
}
