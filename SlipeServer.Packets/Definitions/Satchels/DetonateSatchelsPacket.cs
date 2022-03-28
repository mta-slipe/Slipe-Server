using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Satchels;

public class DetonateSatchelsPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_DETONATE_SATCHELS;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public ushort Latency { get; set; }

    public DetonateSatchelsPacket(uint elementId, ushort latency)
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

        builder.WriteElementId(this.ElementId);
        builder.Write(this.Latency);

        return builder.Build();
    }
}
