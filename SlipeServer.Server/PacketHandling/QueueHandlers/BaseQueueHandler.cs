using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public struct PacketQueueEntry
    {
        public Client Client { get; set; }
        public PacketId PacketId { get; set; }
        public byte[] Data { get; set; }
    }

    public abstract class BaseQueueHandler: IQueueHandler
    {
        protected readonly ConcurrentQueue<PacketQueueEntry> packetQueue;
        public abstract IEnumerable<PacketId> SupportedPacketIds { get; }

        public int QueuedPacketCount => packetQueue.Count;

        public BaseQueueHandler()
        {
            this.packetQueue = new ConcurrentQueue<PacketQueueEntry>();
        }

        public virtual void EnqueuePacket(Client client, PacketId packetId, byte[] data)
        {
            this.packetQueue.Enqueue(new PacketQueueEntry()
            {
                Client = client,
                PacketId = packetId,
                Data = data
            });
        }
    }
}
