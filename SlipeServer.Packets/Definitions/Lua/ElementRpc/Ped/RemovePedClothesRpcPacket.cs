using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Ped;

public class RemovePedClothesRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public byte Type { get; }

    public RemovePedClothesRpcPacket(uint elementId, byte type)
    {
        this.ElementId = elementId;
        this.Type = type;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.REMOVE_PED_CLOTHES);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.Type);

        return builder.Build();
    }
}
