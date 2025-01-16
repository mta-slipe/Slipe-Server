using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.CollisionShape;

public sealed class SetCollisionShapeRadiusRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public float Radius { get; set; }

    public SetCollisionShapeRadiusRpcPacket()
    {

    }

    public SetCollisionShapeRadiusRpcPacket(ElementId elementId, float radius)
    {
        this.ElementId = elementId;
        this.Radius = radius;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_COLSHAPE_RADIUS);
        builder.Write(this.ElementId);

        builder.Write(this.Radius);

        return builder.Build();
    }
}
