using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.WorldObject;

public class MoveObjectRpcPacket(ElementId elementId, PositionRotationAnimation movement) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.MOVE_OBJECT);
        builder.Write(elementId);
        builder.Write(movement);

        return builder.Build();
    }
}
