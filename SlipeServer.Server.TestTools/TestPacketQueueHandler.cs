using SlipeServer.Packets;
using SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling;
using System;

namespace SlipeServer.Server.TestTools;

public class TestPacketQueueHandlerDispatcher
{
    public event Action? Flushed;

    public void Flush()
    {
        Flushed?.Invoke();
    }
}

public class TestPacketQueueHandler<T> : BasePacketQueueHandler<T> where T : Packet
{
    private readonly TestPacketQueueHandlerDispatcher testPacketQueueHandlerDispatcher;
    private readonly IPacketHandler<T> packetHandler;

    public TestPacketQueueHandler(TestPacketQueueHandlerDispatcher testPacketQueueHandlerDispatcher, IPacketHandler<T> packetHandler)
    {
        this.testPacketQueueHandlerDispatcher = testPacketQueueHandlerDispatcher;
        this.packetHandler = packetHandler;

        testPacketQueueHandlerDispatcher.Flushed += HandleFlushed;
    }

    private void HandleFlushed()
    {
        while (this.packetQueue.TryDequeue(out var queueEntry))
        {
            try
            {
                ClientContext.Current = queueEntry.Client;
                this.packetHandler.HandlePacket(queueEntry.Client, queueEntry.Packet);
                TriggerPacketHandled(queueEntry.Packet);
            }
            finally
            {
                ClientContext.Current = null;
            }
        }
    }

    public override void Dispose()
    {
        testPacketQueueHandlerDispatcher.Flushed -= HandleFlushed;
        base.Dispose();
    }
}
