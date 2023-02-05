using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class SetBlipOrderingRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public short Ordering { get; set; }

    public SetBlipOrderingRpcPacket()
    {

    }

    public SetBlipOrderingRpcPacket(ElementId elementId, short ordering)
    {
        this.ElementId = elementId;
        this.Ordering = ordering;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_BLIP_ORDERING);
        builder.Write(this.ElementId);
        builder.WriteCompressed(this.Ordering);

        return builder.Build();
    }
}
