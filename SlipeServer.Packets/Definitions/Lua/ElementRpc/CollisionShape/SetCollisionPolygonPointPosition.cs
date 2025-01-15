using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.CollisionShape;

public sealed class SetCollisionPolygonPointPosition : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public uint Index { get; set; }
    public Vector2 Position { get; set; }

    public SetCollisionPolygonPointPosition(ElementId elementId, uint index, Vector2 position)
    {
        this.ElementId = elementId;
        this.Index = index;
        this.Position = position;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.UPDATE_COLPOLYGON_POINT);
        builder.Write(this.ElementId);

        builder.WriteVector2(this.Position);
        builder.Write(this.Index);

        return builder.Build();
    }
}
