using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Reducing;
using System;
using System.Collections.Generic;
using System.Threading;

#pragma warning disable IDE0130 // Namespace does not match folder structure, for backwards compatibility
namespace SlipeServer.Server.PacketHandling;
#pragma warning restore IDE0130 // Namespace does not match folder structure, for backwards compatibility

/// <summary>
/// Class responsible for routing packets to the appropriate queues
/// </summary>
[Obsolete("PacketReducer is deprecated, consider using ConcurrentPacketRouter instead.", false)]
public class PacketReducer(ILogger<PacketReducer> logger) : IPacketReducer
{
    private readonly Lock @lock = new();
    private readonly Dictionary<PacketId, List<IRegisteredHandler>> registeredPacketHandlerActions = new();

    public void EnqueuePacket(IClient client, PacketId packetId, byte[] data)
    {
        if (this.registeredPacketHandlerActions.TryGetValue(packetId, out var handlers))
        {
            foreach (var handler in handlers)
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
        lock (this.@lock)
        {
            if (!this.registeredPacketHandlerActions.ContainsKey(packetId))
            {
                this.registeredPacketHandlerActions[packetId] = new();
            }

            var pool = new PacketPool<TPacket>();
            this.registeredPacketHandlerActions[packetId].Add(new RegisteredHandler<TPacket>(handler, pool));
            handler.PacketHandled += pool.ReturnPacket;

            void handleDisposed()
            {
                handler.PacketHandled -= pool.ReturnPacket;
                handler.Disposed -= handleDisposed;
            }

            handler.Disposed += handleDisposed;
        }
    }

    public void RemovePacketHandler<TPacket>(PacketId packetId, IPacketQueueHandler<TPacket> handler) where TPacket : Packet, new()
    {
        if (this.registeredPacketHandlerActions.TryGetValue(packetId, out var handlers))
        {
            foreach (var registeredHandler in handlers)
            {
                if (registeredHandler.MatchesHandler(handler))
                {
                    handlers.Remove(registeredHandler);
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
