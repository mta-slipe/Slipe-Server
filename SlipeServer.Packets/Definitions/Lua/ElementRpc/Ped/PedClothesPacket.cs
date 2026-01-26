using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Ped;

public sealed class PedClothesPacket(ElementId elementId, PedClothing[] clothing) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_CLOTHES;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; } = elementId;
    public PedClothing[] Clothing { get; set; } = clothing;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write(this.ElementId);
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
