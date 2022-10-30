using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Vehicles;

public class UnoccupiedVehicleSyncStartPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_UNOCCUPIED_VEHICLE_STARTSYNC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 TurnVelocity { get; set; }
    public float Health { get; set; }


    public UnoccupiedVehicleSyncStartPacket()
    {

    }

    public override void Read(byte[] bytes)
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.WriteElementId(this.ElementId);
        builder.Write(this.Position);
        builder.Write(this.Rotation);
        builder.Write(this.Velocity);
        builder.Write(this.TurnVelocity);
        builder.Write(this.Health);

        return builder.Build();
    }

    public override void Reset()
    {

    }
}
