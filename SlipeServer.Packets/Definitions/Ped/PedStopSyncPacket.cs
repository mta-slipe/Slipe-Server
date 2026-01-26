using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Ped;

public sealed class PedStopSyncPacket(ElementId sourceElementId) : Packet
{
    public override PacketId PacketId { get; } = PacketId.PACKET_ID_PED_STOPSYNC;
    public override PacketReliability Reliability { get; } = PacketReliability.ReliableSequenced;
    public override PacketPriority Priority { get; } = PacketPriority.High;

    public ElementId SourceElementId { get; set; } = sourceElementId;

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
