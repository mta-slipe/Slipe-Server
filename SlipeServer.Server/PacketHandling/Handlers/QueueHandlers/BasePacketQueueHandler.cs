using SlipeServer.Packets;
using SlipeServer.Server.Clients;
using System;
using System.Collections.Concurrent;

namespace SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;

public struct PacketQueueEntry<T>
{
    public IClient Client { get; set; }
    public T Packet { get; set; }
}

public abstract class BasePacketQueueHandler<T> : IPacketQueueHandler<T> where T : Packet
{
    protected readonly ConcurrentQueue<PacketQueueEntry<T>> packetQueue = [];

    public virtual int QueuedPacketCount => this.packetQueue.Count;

    public virtual void EnqueuePacket(IClient client, T packet)
    {
        this.packetQueue.Enqueue(new PacketQueueEntry<T>()
        {
            Client = client,
            Packet = packet
        });
    }

    protected void TriggerPacketHandled(T packet) => this.PacketHandled?.Invoke(packet);

    public virtual void Dispose()
    {
        this.packetQueue.Clear();
        Disposed?.Invoke();

        GC.SuppressFinalize(this);
    }

    public event Action<T>? PacketHandled;
    public event Action? Disposed;
}
