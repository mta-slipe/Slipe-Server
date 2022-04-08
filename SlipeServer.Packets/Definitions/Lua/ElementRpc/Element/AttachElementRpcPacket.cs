using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class AttachElementRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public uint AttachedToElementId { get; set; }
    public Vector3 OffsetPosition { get; set; }
    public Vector3 OffsetRotation { get; set; }

    public AttachElementRpcPacket(uint elementId, uint attachedToElementId, Vector3 offsetPosition, Vector3 offsetRotation)
    {
        this.ElementId = elementId;
        this.AttachedToElementId = attachedToElementId;
        this.OffsetPosition = offsetPosition;
        this.OffsetRotation = offsetRotation;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.ATTACH_ELEMENTS);
        builder.WriteElementId(this.ElementId);
        builder.WriteElementId(this.AttachedToElementId);
        builder.Write(this.OffsetPosition);
        builder.Write(this.OffsetRotation);

        return builder.Build();
    }
}
