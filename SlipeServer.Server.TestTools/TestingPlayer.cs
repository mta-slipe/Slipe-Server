using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System;
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

public class ClientPlayer
{
    public ClientEnvironment ClientEnvironment { get; }

    public event Action<ClientPlayer, Vector3, EventSide>? PositionChanged;
    public Vector3 Position { get; private set; }

    public ClientPlayer(ClientEnvironment clientEnvironment)
    {
        this.ClientEnvironment = clientEnvironment;
    }

    public void SetPosition(Vector3 position, EventSide eventSide = EventSide.Client)
    {
        this.Position = position;
        this.PositionChanged?.Invoke(this, position, eventSide);
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

        this.ClientEnvironment.Client.SendPacket(packet);
    }
}

public class ClientEnvironment
{
    public TestingServer TestingServer { get; }
    public TestingPlayer ServerPlayer { get; }
    public ClientPlayer ClientPlayer { get; }
    public TestingClient Client { get; }
    public TimeContext TimeContext { get; }

    public ClientEnvironment(TestingServer testingServer, TestingPlayer testingPlayer)
    {
        this.TestingServer = testingServer;
        this.ServerPlayer = testingPlayer;
        this.ClientPlayer = new ClientPlayer(this);
        this.Client = testingPlayer.Client as TestingClient;
        this.TimeContext = new();
        this.Client.PacketSent += HandlePacketSent;
        this.ServerPlayer.Disconnected += HandleDisconnected;
        //TestingServer.NetWrapperMock
    }

    private void HandleDisconnected(Player sender, PlayerQuitEventArgs e)
    {
        if(sender.Client is TestingClient testingClient)
            testingClient.PacketSent -= HandlePacketSent;
    }

    private void HandlePacketSent(TestingClient client, Packet packet)
    {
        switch (packet)
        {
            case SetElementPositionRpcPacket setElementPositionRpcPacket:
                if (setElementPositionRpcPacket.ElementId == this.ServerPlayer.Id)
                {
                    this.ClientPlayer.SetPosition(setElementPositionRpcPacket.Position, EventSide.Server);
                }
                else
                {
                    throw new InvalidOperationException($"Element with ID {setElementPositionRpcPacket.ElementId} does not match Player ID {this.ServerPlayer.Id}. Ensure the correct packet is being sent.");
                }
                break;
        }
    }
}
