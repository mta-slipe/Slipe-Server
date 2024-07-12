using SlipeServer.Packets;
using SlipeServer.Server.Clients;
using SlipeServer.Server.PacketHandling.Handlers;
using System;

namespace SlipeServer.Server.Debugging.PacketRecording;

public class PacketRecorderHandler<TPacket> : IPacketQueueHandler<TPacket> where TPacket : Packet
{
    private readonly IClient client;
    private readonly Action<Packet> handler;

    public PacketRecorderHandler(IClient client, Action<Packet> handler)
    {
        this.client = client;
        this.handler = handler;
    }

    public event Action<TPacket>? PacketHandled;
    public event Action? Disposed;

    public void EnqueuePacket(IClient client, TPacket packet)
    {
        if (this.client == client)
        {
            this.handler(packet);
        }
    }

    public void Dispose()
    {
        Disposed?.Invoke();
    }
}
