using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Resources;

public sealed class ResourceStopPacket(
    ushort netId
    ) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_RESOURCE_STOP;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ushort NetId { get; } = netId;

    public override void Read(byte[] bytes)
    {
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.NetId);

        return builder.Build();
    }
}
