using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.WorldObject;

public sealed class SetWorldObjectIsBreakableRpcPacket(ElementId elementId, bool isBreakable) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; } = elementId;
    public bool IsBreakable { get; } = isBreakable;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_OBJECT_BREAKABLE);
        builder.Write(this.ElementId);
        builder.Write(this.IsBreakable);
        return builder.Build();
    }
}
