using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MtaServer.Server.PacketHandling.QueueHandlers
{
    public abstract class WorkerBasedQueueHandler : BaseQueueHandler
    {
        protected readonly MtaServer server;
        protected readonly int sleepInterval;

        public WorkerBasedQueueHandler(MtaServer server, int sleepInterval = 10, int workerCount = 1) : base()
        {
            this.server = server;
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
                await Task.Delay(this.sleepInterval);
            }
        }

        protected abstract void HandlePacket(PacketQueueEntry queueEntry);

    }
}
