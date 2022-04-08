using SlipeServer.Packets;
using System;
using System.Collections.Concurrent;

namespace SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;

public struct PacketQueueEntry<T>
{
    public Client Client { get; set; }
    public T Packet { get; set; }
}

public abstract class BasePacketQueueHandler<T> : IPacketQueueHandler<T> where T : Packet
{
    protected readonly ConcurrentQueue<PacketQueueEntry<T>> packetQueue;

    public virtual int QueuedPacketCount => this.packetQueue.Count;

    public BasePacketQueueHandler()
    {
        this.packetQueue = new();
    }

    public virtual void EnqueuePacket(Client client, T packet)
    {
        this.packetQueue.Enqueue(new PacketQueueEntry<T>()
        {
            Client = client,
            Packet = packet
        });
    }

    protected void TriggerPacketHandled(T packet) => this.PacketHandled?.Invoke(packet);

    public event Action<T>? PacketHandled;
}
