using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Vehicles;

public sealed class VehicleTrailerSyncPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_VEHICLE_TRAILER;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId VehicleId { get; set; }
    public ElementId AttachedVehicleId { get; set; }
    public bool IsAttached { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 TurnVelocity { get; set; }


    public VehicleTrailerSyncPacket()
    {

    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        this.VehicleId = reader.GetElementId();
        this.AttachedVehicleId = reader.GetElementId();
        this.IsAttached = reader.GetBit();

        if (this.IsAttached)
        {
            this.Position = reader.GetVector3WithZAsFloat();
            this.Rotation = reader.GetVehicleRotation();
            this.TurnVelocity = reader.GetVelocityVector();
        }
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.VehicleId);
        builder.Write(this.AttachedVehicleId);
        builder.Write(this.IsAttached);

        if (this.IsAttached)
        {
            builder.WriteVector3WithZAsFloat(this.Position);
            builder.WriteVehicleRotation(this.Rotation);
            builder.WriteVelocityVector(this.TurnVelocity);
        }

        return builder.Build();
    }

    public override void Reset()
    {

    }
}
