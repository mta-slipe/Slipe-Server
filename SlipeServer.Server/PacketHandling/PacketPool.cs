using SlipeServer.Packets;
using System.Collections.Concurrent;

namespace SlipeServer.Server.PacketHandling;

/// <summary>
/// Object pool for packets, allowing for packets to be reused instead of continuously be reallocated. 
/// Using this reduces the amount of garbage collection invocations.
/// </summary>
public class PacketPool<T>(int? maxPacketCount = -1) where T : Packet, new()
{
    private readonly ConcurrentQueue<T> packets = new();

    public T GetPacket()
    {
        if (this.packets.TryDequeue(out var packet))
            return packet;

        return new T();
    }

    public void ReturnPacket(T packet)
    {
        if (this.packets.Count < maxPacketCount || maxPacketCount == -1)
        {
            packet.Reset();
            this.packets.Enqueue(packet);
        }
    }
}
