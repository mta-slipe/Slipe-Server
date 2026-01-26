using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Vehicles;

public sealed class UnoccupiedVehicleSyncStopPacket(ElementId id) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_UNOCCUPIED_VEHICLE_STOPSYNC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId Id { get; set; } = id;

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.Id);

        return builder.Build();
    }

    public override void Reset()
    {

    }
}
