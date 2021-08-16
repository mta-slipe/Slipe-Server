using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{

    public abstract class OrderedScalingWorkerBasedQueueHandler : ScalingWorkerBasedQueueHandler
    {
        private readonly ConcurrentQueue<Client> clientPacketQueue;
        private readonly ConcurrentDictionary<Client, Queue<PacketQueueEntry>> packetsPerClient;

        public override int QueuedPacketCount => this.packetsPerClient.Values.Sum(x => x.Count);

        public OrderedScalingWorkerBasedQueueHandler(
            int sleepInterval = 10, 
            int minWorkerCount = 1, 
            int maxWorkerCount = 10,
            int queueHighThreshold = 20,
            int queueLowThreshold = 3,
            int newWorkerTimeout = 1000
        ) : base(sleepInterval, minWorkerCount, maxWorkerCount, queueHighThreshold, queueLowThreshold, newWorkerTimeout)
        {
            this.clientPacketQueue = new();
            this.packetsPerClient = new();
        }

        public OrderedScalingWorkerBasedQueueHandler(QueueHandlerScalingConfig config, int sleepInterval = 10)
            : this(sleepInterval, config.MinWorkerCount, config.MaxWorkerCount, config.QueueHighThreshold, config.QueueLowThreshold, config.NewWorkerTimeout)
        {

        }

        protected override async void PulsePacketTask(Worker worker)
        {
            while(worker.Active)
            {
                while (this.clientPacketQueue.TryDequeue(out Client? client))
                {
                    while (this.packetsPerClient[client].TryDequeue(out PacketQueueEntry queueEntry))
                    {
                        if (!this.PacketTypes.ContainsKey(queueEntry.PacketId))
                        {
                            throw new NotSupportedException($"Unsupported packet id {queueEntry.PacketId}");
                        }

                        try
                        {
                            var type = this.PacketTypes[queueEntry.PacketId];
                            if (Activator.CreateInstance(type) is Packet packet)
                            {
                                packet.Read(queueEntry.Data);
                                await HandlePacket(queueEntry.Client, packet);
                            }
                        }
                        catch (Exception e)
                        {
                            if (!this.PacketTypes.ContainsKey(queueEntry.PacketId))
                            {
                                throw new NotSupportedException($"Error handling packet id {queueEntry.PacketId}", e);
                            }
                        }
                    }

                    lock (client)
                    {
                        this.packetsPerClient.Remove(client, out var queue);
                    }
                }

                if (this.pulseTaskCompletionSource != null)
                {
                    this.pulseTaskCompletionSource.TrySetResult(0);
                    this.pulseTaskCompletionSource = null;
                }

                await Task.Delay(this.sleepInterval);
            }
        }

        public override void EnqueuePacket(Client client, PacketId packetId, byte[] data)
        {
            PacketQueueEntry entry = new()
            {
                Client = client,
                PacketId = packetId,
                Data = data
            };

            lock (client)
            {
                if (this.packetsPerClient.ContainsKey(client))
                {
                    this.packetsPerClient[client].Enqueue(entry);
                } else
                {
                    this.packetsPerClient[client] = new(new PacketQueueEntry[] { entry });
                    this.clientPacketQueue.Enqueue(client);
                }
            }
        }
    }
}
