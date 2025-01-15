using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.CollisionShape;

public sealed class AddCollisionPolygonPointRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public Vector2 Position { get; set; }
    public uint? Index { get; set; }

    public AddCollisionPolygonPointRpcPacket(ElementId elementId, Vector2 position)
    {
        this.ElementId = elementId;
        this.Position = position;
    }
    public AddCollisionPolygonPointRpcPacket(ElementId elementId, Vector2 position, uint index)
    {
        this.ElementId = elementId;
        this.Position = position;
        this.Index = index;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.ADD_COLPOLYGON_POINT);
        builder.Write(this.ElementId);
        builder.WriteVector2(this.Position);
        if (this.Index.HasValue)
            builder.Write(this.Index);

        return builder.Build();
    }
}
