using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Satchels;

public sealed class DestroySatchelsPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_DESTROY_SATCHELS;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }

    public DestroySatchelsPacket(ElementId elementId)
    {
        this.ElementId = elementId;
    }

    public DestroySatchelsPacket()
    {

    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.ElementId);

        return builder.Build();
    }
}
