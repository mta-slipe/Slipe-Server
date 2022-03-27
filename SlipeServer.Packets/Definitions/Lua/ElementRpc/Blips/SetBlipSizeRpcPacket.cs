using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class SetBlipSizeRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public byte Size { get; set; }

    public SetBlipSizeRpcPacket()
    {

    }

    public SetBlipSizeRpcPacket(uint elementId, byte size)
    {
        this.ElementId = elementId;
        this.Size = size;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_BLIP_SIZE);
        builder.WriteElementId(this.ElementId);
        builder.WriteCapped(this.Size, 5);

        return builder.Build();
    }
}
