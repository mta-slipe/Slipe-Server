using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.CollisionShape;

public class SetCollisionShapeSizeRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public Vector3 Size { get; set; }

    public SetCollisionShapeSizeRpcPacket(ElementId elementId, Vector3 size)
    {
        this.ElementId = elementId;
        this.Size = size;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_COLSHAPE_SIZE);
        builder.Write(this.ElementId);

        builder.WriteCompressedVector3(this.Size);

        return builder.Build();
    }
}
