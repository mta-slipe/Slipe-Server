using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.PacketHandling.Handlers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SlipeServer.Server.PacketHandling.Reducing;

/// <summary>
/// Class responsible for routing packets to the appropriate queues
/// </summary>
public partial class LoggingConcurrentPacketReducer(ILogger<LoggingConcurrentPacketReducer> logger) : IPacketReducer
{
    private static readonly JsonSerializerOptions jsonOptions = new() { Converters = { new LuaValueJsonConverter() } };

    private readonly ConcurrentDictionary<PacketId, ConcurrentDictionary<IRegisteredHandler, byte>> registeredPacketHandlerActions = [];

    public void EnqueuePacket(IClient client, PacketId packetId, byte[] data)
    {
        if (this.registeredPacketHandlerActions.TryGetValue(packetId, out var handlers))
        {
            var name = NameSanitisingRegex().Replace(client.Player.Name, "");
            var directory = $"packetlog/{name}/{packetId}";
            Directory.CreateDirectory(directory);
            File.WriteAllBytes($"{directory}/{(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds}.packet.binary", data);

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

    private sealed class LuaValueJsonConverter : JsonConverter<LuaValue>
    {
        public override LuaValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException();

        public override void Write(Utf8JsonWriter writer, LuaValue value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());

        public override void WriteAsPropertyName(Utf8JsonWriter writer, LuaValue value, JsonSerializerOptions options)
            => writer.WritePropertyName(value.ToString());
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

            var name = NameSanitisingRegex().Replace(client.Player.Name, "");
            var directory = $"packetlog/{name}/{packet.PacketId}";
            Directory.CreateDirectory(directory);
            File.WriteAllText($"{directory}/{(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds}-{typeof(Packet).Name}.packet.json", JsonSerializer.Serialize(packet, jsonOptions));
        }

        public bool MatchesHandler(object otherHandler) => handler == otherHandler;
    }

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NameSanitisingRegex();
}
