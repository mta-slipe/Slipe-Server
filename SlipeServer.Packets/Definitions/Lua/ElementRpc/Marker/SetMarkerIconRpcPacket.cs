using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Marker;

public class SetMarkerIconRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public byte MarkerType { get; set; }
    public SetMarkerIconRpcPacket(ElementId elementId, byte markerType)
    {
        this.ElementId = elementId;
        this.MarkerType = markerType;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_MARKER_ICON);
        builder.Write(this.ElementId);
        builder.Write(this.MarkerType);

        return builder.Build();
    }
}
