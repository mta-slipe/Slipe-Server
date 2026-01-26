using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class AttachElementRpcPacket(ElementId elementId, ElementId attachedToElementId, Vector3 offsetPosition, Vector3 offsetRotation) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; } = elementId;
    public ElementId AttachedToElementId { get; set; } = attachedToElementId;
    public Vector3 OffsetPosition { get; set; } = offsetPosition;
    public Vector3 OffsetRotation { get; set; } = offsetRotation;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.ATTACH_ELEMENTS);
        builder.Write(this.ElementId);
        builder.Write(this.AttachedToElementId);
        builder.Write(this.OffsetPosition);
        builder.Write(this.OffsetRotation);

        return builder.Build();
    }
}
