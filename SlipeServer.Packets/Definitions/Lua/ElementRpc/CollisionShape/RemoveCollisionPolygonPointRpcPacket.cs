using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.CollisionShape;

public sealed class RemoveCollisionPolygonPointRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public uint Index { get; set; }

    public RemoveCollisionPolygonPointRpcPacket(ElementId elementId, uint index)
    {
        this.ElementId = elementId;
        this.Index = index;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.REMOVE_COLPOLYGON_POINT);
        builder.Write(this.ElementId);
        builder.Write(this.Index);

        return builder.Build();
    }
}
