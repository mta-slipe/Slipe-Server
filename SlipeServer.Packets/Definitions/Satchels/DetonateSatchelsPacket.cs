using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Satchels;

public sealed class DetonateSatchelsPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_DETONATE_SATCHELS;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public ushort Latency { get; set; }

    public DetonateSatchelsPacket(ElementId elementId, ushort latency)
    {
        this.ElementId = elementId;
        this.Latency = latency;
    }

    public DetonateSatchelsPacket()
    {

    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.ElementId);
        builder.Write(this.Latency);

        return builder.Build();
    }
}
