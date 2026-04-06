using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.WorldObject;

public sealed class SetWorldObjectDoubleSidedRpcPacket(ElementId elementId, bool doubleSided) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; } = elementId;
    public bool DoubleSided { get; } = doubleSided;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_DOUBLESIDED);
        builder.Write(this.ElementId);
        builder.Write(this.DoubleSided);
        return builder.Build();
    }
}
