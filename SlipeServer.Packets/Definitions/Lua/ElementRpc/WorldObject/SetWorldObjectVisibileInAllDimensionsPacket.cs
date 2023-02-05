using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.WorldObject;

public class SetWorldObjectVisibileInAllDimensionsPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; }
    public bool IsVisibleInAllDimensions { get; }

    public SetWorldObjectVisibileInAllDimensionsPacket(ElementId elementId, bool isVisibleInAllDimensions)
    {
        this.ElementId = elementId;
        this.IsVisibleInAllDimensions = isVisibleInAllDimensions;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_OBJECT_VISIBLE_IN_ALL_DIMENSIONS);
        builder.Write(this.ElementId);
        builder.Write(this.IsVisibleInAllDimensions);

        return builder.Build();
    }
}
