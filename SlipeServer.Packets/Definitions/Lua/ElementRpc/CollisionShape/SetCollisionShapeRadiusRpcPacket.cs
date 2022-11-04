using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.CollisionShape;

public class SetCollisionShapeRadiusRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public float Radius { get; set; }

    public SetCollisionShapeRadiusRpcPacket()
    {

    }

    public SetCollisionShapeRadiusRpcPacket(uint elementId, float radius)
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
        builder.WriteElementId(this.ElementId);

        builder.Write(this.Radius);

        return builder.Build();
    }
}
