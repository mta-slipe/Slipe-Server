using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Drawing;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Marker;

public sealed class SetMarkerColorRpcPacket(ElementId elementId, Color color) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; } = elementId;
    public Color Color { get; set; } = color;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_MARKER_COLOR);
        builder.Write(this.ElementId);
        builder.WriteBgra(this.Color);

        return builder.Build();
    }
}
