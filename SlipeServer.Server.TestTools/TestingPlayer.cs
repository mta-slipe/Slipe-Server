using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements;
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

public class ClientEnvironment
{
    public TestingPlayer Player { get; }

    public Vector3 Position { get; private set; }

    public ClientEnvironment(TestingPlayer testingPlayer)
    {
        this.Player = testingPlayer;
        var testingClient = testingPlayer.Client as TestingClient;
        testingClient.PacketSent += HandlePacketSent;
    }

    private void HandlePacketSent(TestingClient client, Packets.Packet packet)
    {
        switch (packet)
        {
            case SetElementPositionRpcPacket setElementPositionRpcPacket:
                if (setElementPositionRpcPacket.ElementId == this.Player.Id)
                {
                    this.Position = setElementPositionRpcPacket.Position;
                } else
                {
                    throw new Exception("Element not found.");
                }
                break;
        }
    }
}
