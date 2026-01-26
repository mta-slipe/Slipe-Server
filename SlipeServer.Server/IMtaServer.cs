using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Serving;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server;

public interface IMtaServer
{
    Action? BuildFinalizer { get; }
    Configuration Configuration { get; }
    string GameType { get; set; }
    bool HasPassword { get; }
    bool IsRunning { get; }
    string MapName { get; set; }
    string? Password { get; set; }
    IEnumerable<Player> Players { get; }
    RootElement RootElement { get; }
    IServiceProvider Services { get; }
    DateTime StartDatetime { get; }
    TimeSpan Uptime { get; }

    event Action<IClient>? ClientConnected;
    event Action<Element>? ElementCreated;
    event Action<LuaEvent>? LuaEventTriggered;
    event Action<ushort>? MaxPlayerCountChanged;
    event Action<Player>? PlayerJoined;
    event Action<IMtaServer>? Started;
    event Action<IMtaServer>? Stopped;

    void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files);
    INetWrapper AddNetWrapper(INetWrapper wrapper, AntiCheatConfiguration? configuration = null);
    INetWrapper AddNetWrapper(string directory, string netDllPath, string host, ushort port, uint expectedVersion, uint expectedVersionType = 9, AntiCheatConfiguration? configuration = null);
    void AddResourceServer(IResourceServer resourceServer);
    T AssociateElement<T>(T element) where T : Element;
    void BroadcastPacket(Packet packet);
    void EnqueuePacketToClient(IClient client, PacketId packetId, byte[] data);
    Action ForAny<TElement>(Action<TElement> action, Action<TElement>? cleanup = null) where TElement : Element;
    TResource GetAdditionalResource<TResource>() where TResource : Resource;
    T GetRequiredService<T>() where T : notnull;
    T GetRequiredServiceScoped<T>() where T : notnull;
    T? GetService<T>();
    void HandleLuaEvent(LuaEvent luaEvent);
    void HandlePlayerJoin(Player player);
    object Instantiate(Type type, params object[] parameters);
    T Instantiate<T>(params object[] parameters);
    object InstantiatePersistent(Type type, params object[] parameters);
    T InstantiatePersistent<T>(params object[] parameters);
    object InstantiateScoped(Type type, params object[] parameters);
    T InstantiateScoped<T>(params object[] parameters);
    T InstantiateScopedPersistent<T>(params object[] parameters);
    void RegisterNetWrapper(INetWrapper netWrapper);
    void RegisterPacketHandler<T>(PacketId packetId, IPacketQueueHandler<T> queueHandler) where T : Packet, new();
    void RegisterPacketHandler<TPacket, TPacketQueueHandler, TPacketHandler>(params object[] parameters)
        where TPacket : Packet, new()
        where TPacketQueueHandler : class, IPacketQueueHandler<TPacket>
        where TPacketHandler : IPacketHandler<TPacket>;
    void RegisterPacketHandler<TPacketHandler, TPacket>(params object[] parameters)
        where TPacketHandler : IPacketHandler<TPacket>
        where TPacket : Packet, new();
    void RemoveAdditionalResource(Resource resource);
    void RemoveElement(Element element);
    void SetMaxPlayers(ushort slots);
    void Start();
    void Stop();
}

public interface IMtaServer<TPlayer> : IMtaServer where TPlayer : Player
{
    new IEnumerable<TPlayer> Players { get; }

    new event Action<TPlayer>? PlayerJoined;
}
