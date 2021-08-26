using SlipeServer.Packets;
using System.Threading.Tasks;

namespace SlipeServer.Server.PacketHandling.Handlers.QueueHandlers
{
    public class WorkerBasedPacketQueueHandler<T> : BasePacketQueueHandler<T> where T : Packet
    {
        private readonly int workerCount;
        private readonly int sleepTime;
        private readonly IPacketHandler<T> packetHandler;
        private TaskCompletionSource<int>? pulseTaskCompletionSource;

        public WorkerBasedPacketQueueHandler(IPacketHandler<T> packetHandler, int workerCount = 1, int sleepTime = 10)
        {
            this.workerCount = workerCount;
            this.sleepTime = sleepTime;
            this.packetHandler = packetHandler;
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
                    this.packetHandler.HandlePacket(queueEntry.Client, queueEntry.Packet);
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
