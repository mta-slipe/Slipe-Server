using SlipeServer.Packets;
using SlipeServer.Server.Clients;
using System;

namespace SlipeServer.Server.PacketHandling.Handlers;

public interface IPacketQueueHandlerBase : IDisposable
{
    event Action Disposed;
}

public interface IPacketQueueHandler<T> : IPacketQueueHandlerBase where T : Packet
{
    void EnqueuePacket(IClient client, T packet);

    event Action<T> PacketHandled;
}
