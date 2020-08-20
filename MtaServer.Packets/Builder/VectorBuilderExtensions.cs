using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Builder
{
    public static class VectorBuilderExtensions
    {
        public static void Write(this PacketBuilder builder, Vector3 vector)
        {
            builder.Write(vector.X);
            builder.Write(vector.Y);
            builder.Write(vector.Z);
        }

        public static void Write(this PacketBuilder builder, Vector2 vector)
        {
            builder.Write(vector.X);
            builder.Write(vector.Y);
        }

        public static void Write(this PacketBuilder builder, Vector3 vector, int integerBits, int fractionalBits)
        {
            builder.WriteFloat(vector.X, integerBits, fractionalBits);
            builder.WriteFloat(vector.Y, integerBits, fractionalBits);
            builder.WriteFloat(vector.Z, integerBits, fractionalBits);
        }

        public static void WriteVector2(this PacketBuilder builder, Vector2 vector, int integerBits = 14, int fractionalBits = 10)
        {
            builder.WriteFloat(vector.X, integerBits, fractionalBits);
            builder.WriteFloat(vector.Y, integerBits, fractionalBits);
        }

        public static void WriteVector3WithZAsFloat(this PacketBuilder builder, Vector3 vector, int integerBits = 14, int fractionalBits = 10)
        {
            builder.WriteFloat(vector.X, integerBits, fractionalBits);
            builder.WriteFloat(vector.Y, integerBits, fractionalBits);
            builder.Write(vector.Z);
        }

        public static void WriteVectorAsUshorts(this PacketBuilder builder, Vector3 vector)
        {
            builder.Write((ushort)(vector.X * (65536 / 360f)));
            builder.Write((ushort)(vector.Y * (65536 / 360f)));
            builder.Write((ushort)(vector.Z * (65536 / 360f)));
        }

        public static void WriteCompressedVector3(this PacketBuilder builder, Vector3 vector)
        {
            var magnitude = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            builder.Write((float)magnitude);
            if (magnitude > 0.00001f)
            {
                builder.WriteCompressed((float)(vector.X / magnitude));
                builder.WriteCompressed((float)(vector.Y / magnitude));
                builder.WriteCompressed((float)(vector.Z / magnitude));
            }
        }

        public static void WriteNormalizedVector(this PacketBuilder builder, Vector3 vector)
        {
            float x = vector.X;
            float y = vector.Y;
            float z = vector.Z;

            builder.Write(x < 0.0);
            if (y == 0.0)
                builder.Write(true);
            else
            {
                builder.Write(false);
                builder.WriteCompressed(y);
            }
            if (z == 0.0)
                builder.Write(true);
            else
            {
                builder.Write(false);
                builder.WriteCompressed(z);
            }
        }

        public static void WriteVelocityVector(this PacketBuilder builder, Vector3 vector)
        {
            builder.Write(vector != Vector3.Zero);
            if (vector.Length() == 0)
                return;

            builder.Write(vector.Length());
            builder.WriteNormalizedVector(Vector3.Normalize(vector));
        }
    }
}
