using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public abstract class WorkerBasedQueueHandler : BaseQueueHandler
    {
        protected abstract Dictionary<PacketId, Type> PacketTypes { get; }
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
                            HandlePacket(queueEntry.Client, packet);
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

        protected abstract void HandlePacket(Client client, Packet packet);

        public Task GetPulseTask()
        {
            this.pulseTaskCompletionSource = new TaskCompletionSource<int>();
            return this.pulseTaskCompletionSource.Task;
        }
    }
}
