using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Rpc;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Handlers.Rpc;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.TestTools;

public class TestingPlayer : Player
{
    public static Player CreateStandalone()
    {
        var netWrapper = new Mock<INetWrapper>();
        var player = new TestingPlayer();
        player.Client = new TestingClient(0, netWrapper.Object, player);
        return player;
    }
}

public enum EventSide
{
    Client,
    Server
}

public class ClientElementCollection
{
    private readonly object @lock = new();
    private readonly Dictionary<ElementId, ClientElement> elements = [];

    public void Add(ClientElement clientElement)
    {
        lock (@lock)
        {
            this.elements.TryAdd(clientElement.ElementId, clientElement);
        }
    }

    public void Remove(ClientElement clientElement)
    {
        lock (@lock)
        {
            this.elements.Remove(clientElement.ElementId);
        }
    }

    public ClientElement Get(ElementId elementId)
    {
        lock (@lock)
        {
            if (this.elements.TryGetValue(elementId, out var clientElement))
                return clientElement;
            throw new KeyNotFoundException($"Element of id {elementId} not found.");
        }
    }
}

public class ClientElement
{
    public TimeContext TimeContext { get; } = new();
    public ElementId ElementId { get; }


    public event Action<ClientElement, Vector3, EventSide>? PositionChanged;
    public Vector3 Position { get; private set; }
    public event Action<ClientElement, string, EventSide>? NameChanged;
    public string Name { get; private set; }

    public ClientElement(ElementId elementId)
    {
        this.ElementId = elementId;
    }

    public void SetPosition(Vector3 position, EventSide eventSide = EventSide.Client)
    {
        this.Position = position;
        this.PositionChanged?.Invoke(this, position, eventSide);
    }

    public void SetName(string name, EventSide eventSide = EventSide.Client)
    {
        this.Name = name;
        this.NameChanged?.Invoke(this, name, eventSide);
    }
}

public class ClientPlayer<TPlayer> : ClientElement where TPlayer : Player
{
    public ClientElementCollection ElementCollection { get; } = new();

    public TestingServer<TPlayer> TestingServer { get; }
    public TPlayer ServerPlayer { get; }
    public TestingClient Client { get; }

    public ClientPlayer(TestingServer<TPlayer> testingServer, TPlayer testingPlayer) : base(testingPlayer.Id)
    {
        this.TestingServer = testingServer;
        this.ServerPlayer = testingPlayer;
        this.Client = testingPlayer.Client as TestingClient;
        this.Client.PacketSent += HandlePacketSent;
        this.ServerPlayer.Disconnected += HandleDisconnected;

        this.ElementCollection.Add(this);
        this.Client.SendPacket(new RpcPacket
        {
            FunctionId = Packets.Enums.RpcFunctions.INITIAL_DATA_STREAM
        });
    }

    private void HandleDisconnected(Player sender, PlayerQuitEventArgs e)
    {
        if (sender.Client is TestingClient testingClient)
            testingClient.PacketSent -= HandlePacketSent;
    }

    private void HandlePacketSent(TestingClient client, Packet packet)
    {
        switch (packet)
        {
            case RpcPacket rpcPacket:
                this.TestingServer.GetRequiredService<RpcPacketHandler>().HandlePacket(client, rpcPacket);
                break;
            case SetElementPositionRpcPacket setElementPositionRpcPacket:
                {
                    var element = this.ElementCollection.Get(setElementPositionRpcPacket.ElementId);
                    element.SetPosition(setElementPositionRpcPacket.Position, EventSide.Server);
                }
                break;
            case AddEntityPacket addEntityPacket:

                break;
            case PlayerListPacket playerListPacket:
                ;
                break;
            case SetSyncSettingsPacket setSyncSettingsPacket:

                break;
            case SetSyncIntervalPacket setSyncIntervalPacket:

                break;
            case PlayerPureSyncPacket playerPureSyncPacket:

                break;
            case ReturnSyncPacket returnSyncPacket:

                break;
            case ChangeNicknamePacket changeNicknamePacket:
                {
                    var element = this.ElementCollection.Get(changeNicknamePacket.PlayerId);
                    element.SetName(changeNicknamePacket.Name, EventSide.Server);
                }

                break;
            default:
                throw new NotImplementedException(packet.ToString());
        }
    }

    /// <summary>
    /// This is reimplementation of this function: <a href="https://github.com/multitheftauto/mtasa-blue/blob/812e0458157db4b0e0a466996547273cfb3afd22/Client/mods/deathmatch/logic/CNetAPI.cpp#L282">void CNetAPI::DoPulse()</a>
    /// </summary>
    public void SynchronizeWithServer()
    {
        var packet = new PlayerPureSyncPacket
        {
            Position = this.Position,
        };

        this.Client.SendPacket(packet);
    }
}
