using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Vehicles;

public class VehicleResyncPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_VEHICLE_RESYNC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 TurnVelocity { get; set; }
    public float Health { get; set; }

    public byte[] DoorStates { get; set; } = Array.Empty<byte>();
    public byte[] WheelStates { get; set; } = Array.Empty<byte>();
    public byte[] PanelStates { get; set; } = Array.Empty<byte>();
    public byte[] LightStates { get; set; } = Array.Empty<byte>();


    public VehicleResyncPacket()
    {

    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.ElementId);
        builder.WriteVector3WithZAsFloat(this.Position);
        builder.WriteVehicleRotation(this.Rotation);
        builder.WriteVelocityVector(this.Velocity);
        builder.WriteVelocityVector(this.TurnVelocity);
        builder.WriteVehicleHealth(this.Health);

        builder.WriteCapped((byte)255, 4);

        WriteComponentStates(builder, this.DoorStates, 3);
        WriteComponentStates(builder, this.WheelStates, 2);
        WriteComponentStates(builder, this.PanelStates, 2);
        WriteComponentStates(builder, this.LightStates, 2);

        return builder.Build();
    }

    private void WriteComponentStates(PacketBuilder builder, byte[] data, int bits)
    {
        builder.Write(data.Any(x => x > 0));
        foreach (var d in data)
            builder.WriteCapped(d, bits);
    }

    public override void Reset()
    {

    }
}
