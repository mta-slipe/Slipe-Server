using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Ped;

public class PedClothesPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_CLOTHES;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public PedClothing[] Clothing { get; set; }

    public PedClothesPacket(uint elementId, PedClothing[] clothing)
    {
        this.ElementId = elementId;
        this.Clothing = clothing;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.WriteElementId(this.ElementId);
        builder.Write((ushort)this.Clothing.Length);
        foreach (var item in this.Clothing)
        {
            builder.WriteStringWithByteAsLength(item.Texture);
            builder.WriteStringWithByteAsLength(item.Model);
            builder.Write(item.Type);
        }

        return builder.Build();
    }
}
