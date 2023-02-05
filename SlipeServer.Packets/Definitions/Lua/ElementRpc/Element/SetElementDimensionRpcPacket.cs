using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class SetElementDimensionRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public ushort Dimension { get; set; }

    public SetElementDimensionRpcPacket()
    {

    }

    public SetElementDimensionRpcPacket(ElementId elementId, ushort dimension)
    {
        this.ElementId = elementId;
        this.Dimension = dimension;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_DIMENSION);
        builder.Write(this.ElementId);

        builder.Write(this.Dimension);

        return builder.Build();
    }
}
