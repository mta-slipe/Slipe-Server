using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class DetachElementRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public Vector3 OffsetPosition { get; set; }

    public DetachElementRpcPacket(ElementId elementId, Vector3 offsetPosition)
    {
        this.ElementId = elementId;
        this.OffsetPosition = offsetPosition;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.DETACH_ELEMENTS);
        builder.Write(this.ElementId);
        builder.Write(this.OffsetPosition);

        return builder.Build();
    }
}
