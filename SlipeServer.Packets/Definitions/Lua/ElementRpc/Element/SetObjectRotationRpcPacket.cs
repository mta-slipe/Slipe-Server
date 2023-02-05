using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class SetObjectRotationRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public Vector3 Rotation { get; set; }

    public SetObjectRotationRpcPacket()
    {

    }

    public SetObjectRotationRpcPacket(ElementId elementId, Vector3 rotation)
    {
        this.ElementId = elementId;
        this.Rotation = rotation;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_OBJECT_ROTATION);
        builder.Write(this.ElementId);

        builder.Write(this.Rotation);

        return builder.Build();
    }
}
