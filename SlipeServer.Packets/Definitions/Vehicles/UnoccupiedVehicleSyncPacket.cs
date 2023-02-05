using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;

namespace SlipeServer.Packets.Definitions.Vehicles;

public class UnoccupiedVehicleSyncPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_UNOCCUPIED_VEHICLE_SYNC;

    public override PacketReliability Reliability => PacketReliability.UnreliableSequenced;

    public override PacketPriority Priority => PacketPriority.Medium;

    public List<UnoccupiedVehicleSync> Vehicles { get; set; }


    public UnoccupiedVehicleSyncPacket()
    {
        this.Vehicles = new();
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        while (reader.Size - reader.Counter > 16)
        {
            var id = reader.GetElementId();
            var timeContext = reader.GetByte();
            var flags = (UnoccupiedVehicleSyncFlags)BitConverter.ToUInt16(reader.GetBytesCapped(9));
            this.Vehicles.Add(new UnoccupiedVehicleSync()
            {
                Id = id,
                TimeContext = timeContext,
                Flags = flags,
                Position = ((flags & UnoccupiedVehicleSyncFlags.Position) > 0)
                    ? reader.GetVector3WithZAsFloat() : null,
                Rotation = ((flags & UnoccupiedVehicleSyncFlags.Rotation) > 0)
                    ? reader.GetVehicleRotation() : null,
                Velocity = ((flags & UnoccupiedVehicleSyncFlags.Velocity) > 0)
                    ? reader.GetVelocityVector() : null,
                TurnVelocity = ((flags & UnoccupiedVehicleSyncFlags.TurnVelocity) > 0)
                    ? reader.GetVelocityVector() : null,
                Health = ((flags & UnoccupiedVehicleSyncFlags.Health) > 0)
                    ? reader.GetVehicleHealth() : null,
                Trailer = ((flags & UnoccupiedVehicleSyncFlags.Trailer) > 0)
                    ? reader.GetElementId() : null
            });
        }
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        foreach (var vehicle in this.Vehicles)
        {
            UnoccupiedVehicleSyncFlags flags = 0 |
                (vehicle.Position != null ? UnoccupiedVehicleSyncFlags.Position : 0) |
                (vehicle.Rotation != null ? UnoccupiedVehicleSyncFlags.Rotation : 0) |
                (vehicle.Velocity != null ? UnoccupiedVehicleSyncFlags.Velocity : 0) |
                (vehicle.TurnVelocity != null ? UnoccupiedVehicleSyncFlags.TurnVelocity : 0) |
                (vehicle.Health != null ? UnoccupiedVehicleSyncFlags.Health : 0) |
                (vehicle.Trailer != null ? UnoccupiedVehicleSyncFlags.Trailer : 0);
            builder.Write(vehicle.Id);
            builder.Write(vehicle.TimeContext);
            builder.WriteCapped((ushort)flags, 9);

            if (vehicle.Position != null)
                builder.WriteVector3WithZAsFloat(vehicle.Position.Value);
            if (vehicle.Rotation != null)
                builder.WriteVehicleRotation(vehicle.Rotation.Value);
            if (vehicle.Velocity != null)
                builder.WriteVelocityVector(vehicle.Velocity.Value);
            if (vehicle.TurnVelocity != null)
                builder.WriteVelocityVector(vehicle.TurnVelocity.Value);
            if (vehicle.Health != null)
                builder.WriteVehicleHealth(vehicle.Health.Value);
            if (vehicle.Trailer != null)
                builder.Write(vehicle.Trailer.Value);

        }

        return builder.Build();
    }

    public override void Reset()
    {
        this.Vehicles.Clear();
    }
}
