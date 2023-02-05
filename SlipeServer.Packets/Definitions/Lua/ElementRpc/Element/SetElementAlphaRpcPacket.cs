using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class SetElementAlphaRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public byte Alpha { get; set; }

    public SetElementAlphaRpcPacket()
    {

    }

    public SetElementAlphaRpcPacket(ElementId elementId, byte alpha)
    {
        this.ElementId = elementId;
        this.Alpha = alpha;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_ALPHA);
        builder.Write(this.ElementId);

        builder.Write(this.Alpha);

        return builder.Build();
    }
}
