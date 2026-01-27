using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.PacketHandling.Handlers;
using System;
using System.Collections.Concurrent;

namespace SlipeServer.Server.PacketHandling.Reducing;

/// <summary>
/// Class responsible for routing packets to the appropriate queues
/// </summary>
public class ConcurrentPacketReducer(ILogger logger) : IPacketReducer
{
    private readonly ConcurrentDictionary<PacketId, ConcurrentDictionary<IRegisteredHandler, byte>> registeredPacketHandlerActions = [];

    public void EnqueuePacket(IClient client, PacketId packetId, byte[] data)
    {
        if (this.registeredPacketHandlerActions.TryGetValue(packetId, out var handlers))
        {
            foreach (var (handler, _) in handlers)
                try
                {
                    handler.HandlePacket(client, data);
                }
                catch (Exception e)
                {
                    logger.LogError("Enqueueing packet ({packetId}) failed.\n{message}\n{stackTrace}", packetId, e.Message, e.StackTrace);
                }
        } else if (packetId != PacketId.PACKET_ID_PLAYER_NO_SOCKET)
        {
            logger.LogWarning("Received unregistered packet {packetId}", packetId);
        }
    }

    public void RegisterPacketHandler<TPacket>(PacketId packetId, IPacketQueueHandler<TPacket> handler) where TPacket : Packet, new()
    {
        var handlers = this.registeredPacketHandlerActions.GetOrAdd(packetId, _ => []);

        var pool = new PacketPool<TPacket>();
        handlers.TryAdd(new RegisteredHandler<TPacket>(handler, pool), 0);

        handler.PacketHandled += pool.ReturnPacket;

        void handleDisposed()
        {
            handler.PacketHandled -= pool.ReturnPacket;
            handler.Disposed -= handleDisposed;
        }

        handler.Disposed += handleDisposed;
    }

    public void RemovePacketHandler<TPacket>(PacketId packetId, IPacketQueueHandler<TPacket> handler) where TPacket : Packet, new()
    {
        if (this.registeredPacketHandlerActions.TryGetValue(packetId, out var handlers))
        {
            foreach (var (registeredHandler, _) in handlers)
            {
                if (registeredHandler.MatchesHandler(handler))
                {
                    handlers.TryRemove(registeredHandler, out _);
                    break;
                }
            }
        }
    }

    private interface IRegisteredHandler
    {
        void HandlePacket(IClient client, byte[] data);

        bool MatchesHandler(object otherHandler);
    }

    private class RegisteredHandler<T>(IPacketQueueHandler<T> handler, PacketPool<T> pool) : IRegisteredHandler where T : Packet, new()
    {
        public void HandlePacket(IClient client, byte[] data)
        {
            var packet = pool.GetPacket();
            packet.Read(data);
            handler.EnqueuePacket(client, packet);
        }

        public bool MatchesHandler(object otherHandler) => handler == otherHandler;
    }
}
