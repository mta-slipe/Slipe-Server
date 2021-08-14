using System;
using System.Numerics;

namespace SlipeServer.Packets.Reader
{
    public static class VehicleReaderExtensions
    {
        public static byte GetVehicleSeat(this PacketReader reader) => reader.GetByteCapped(4);
        public static float GetVehicleHealth(this PacketReader reader) => reader.GetFloatFromBits(12, 0f, 2047.5f);

        public static Vector3 GetVehicleRotation(this PacketReader reader) => new(
            reader.GetUint16() * (360 / 65536f),
            reader.GetUint16() * (360 / 65536f),
            reader.GetUint16() * (360 / 65536f)
        );

        public static Vector2 GetTurretRotation(this PacketReader reader) => new(
            reader.GetInt16() / (32767.0f / MathF.PI),
            reader.GetInt16() / (32767.0f / MathF.PI)
        );
    }
}
