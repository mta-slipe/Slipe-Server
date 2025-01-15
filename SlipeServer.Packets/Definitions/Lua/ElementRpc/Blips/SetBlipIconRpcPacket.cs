using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class SetBlipIconRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public byte Icon { get; set; }

    public SetBlipIconRpcPacket()
    {

    }

    public SetBlipIconRpcPacket(ElementId elementId, byte icon)
    {
        this.ElementId = elementId;
        this.Icon = icon;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_BLIP_ICON);
        builder.Write(this.ElementId);
        builder.WriteCapped(this.Icon, 6);

        return builder.Build();
    }
}
