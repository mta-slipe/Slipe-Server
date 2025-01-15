using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Voice;

public sealed class VoiceEndPacket : Packet
{
    public override PacketId PacketId { get; } = PacketId.PACKET_ID_VOICE_END;
    public override PacketReliability Reliability { get; } = PacketReliability.ReliableSequenced;
    public override PacketPriority Priority { get; } = PacketPriority.Low;

    public ElementId SourceElementId { get; set; }

    public VoiceEndPacket(ElementId sourceElementId)
    {
        this.SourceElementId = sourceElementId;
    }

    public VoiceEndPacket()
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write(this.SourceElementId);
        return builder.Build();
    }


    public override void Read(byte[] bytes)
    {

    }
}
