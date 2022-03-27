using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Builder;

public static class VehicleBuilderExtensions
{
    public static void WriteVehicleSeat(this PacketBuilder builder, byte seat) => builder.WriteCapped(seat, 4);
    public static void WriteVehicleHealth(this PacketBuilder builder, float health) => builder.WriteFloatFromBits(health, 12, 0f, 2047.5f, false);
    public static void WriteLowPrecisionVehicleHealth(this PacketBuilder builder, float health) => builder.WriteFloatFromBits(health, 8, 0f, 2040, false);

    public static void WriteVehicleRotation(this PacketBuilder builder, Vector3 rotation)
    {
        builder.Write((ushort)(rotation.X * (65536f / 360f)));
        builder.Write((ushort)(rotation.Y * (65536f / 360f)));
        builder.Write((ushort)(rotation.Z * (65536f / 360f)));
    }

    public static void WriteTurretRotation(this PacketBuilder builder, Vector2 rotation)
    {
        builder.Write((short)(rotation.X * (32767.0f / MathF.PI)));
        builder.Write((short)(rotation.Y * (32767.0f / MathF.PI)));
    }
}
