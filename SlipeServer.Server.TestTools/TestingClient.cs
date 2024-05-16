using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements;
using System;
using System.Net.Sockets;

namespace SlipeServer.Server.TestTools;

public class TestingClient : Client<Player>
{
    public uint Address { get; private set; }

    public event Action<TestingClient, Packet> PacketSent;

    public TestingClient(uint address, INetWrapper netWrapper, Player player)
        : base(address, netWrapper, player)
    {
        this.Address = address;
        this.ConnectionState = Enums.ClientConnectionState.Joined;
    }

    public override void SendPacket(Packet packet)
    {
        base.SendPacket(packet);
        PacketSent?.Invoke(this, packet);
    }

    public override void SendPacket(PacketId packetId, byte[] data, PacketPriority priority = PacketPriority.Medium, PacketReliability reliability = PacketReliability.Unreliable)
    {
        base.SendPacket(packetId, data, priority, reliability);
        switch (packetId)
        {
            case PacketId.PACKET_ID_LUA_ELEMENT_RPC:
                var elementRpcFunction = (ElementRpcFunction)data[0];
                switch (elementRpcFunction)
                {
                    case ElementRpcFunction.SET_ELEMENT_POSITION:
                        HandleElementSetElementPositionPacket(data);
                        break;
                }
                ;
                break;
        }
    }

    private void HandleElementSetElementPositionPacket(byte[] data)
    {
        var packet = new SetElementPositionRpcPacket();
        packet.Read(data);
        PacketSent?.Invoke(this, packet);
    }
}
