using SlipeServer.Packets;
using System;

namespace SlipeServer.Server.PacketHandling.Handlers;

public interface IPacketQueueHandler<T> where T : Packet
{
    void EnqueuePacket(IClient client, T packet);

    event Action<T> PacketHandled;
}
