using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Definitions.Vehicles;

public class UnoccupiedVehicleSyncStopPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_UNOCCUPIED_VEHICLE_STOPSYNC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public uint Id { get; set; }

    public UnoccupiedVehicleSyncStopPacket(uint id)
    {
        this.Id = id;
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.WriteElementId(this.Id);

        return builder.Build();
    }

    public override void Reset()
    {

    }
}
