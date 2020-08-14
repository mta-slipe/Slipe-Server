using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Reader
{
    public static class VectorReaderExtensions
    {
        public static Vector3 GetVector3(this PacketReader reader) 
            => new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());

        public static Vector2 GetVector2(this PacketReader reader) 
            => new Vector2(reader.GetFloat(), reader.GetFloat());

        public static Vector3 GetVector3(this PacketReader reader, int integerBits, int fractionalBits)
            => new Vector3(
                reader.GetFloat(integerBits, fractionalBits), 
                reader.GetFloat(integerBits, fractionalBits), 
                reader.GetFloat(integerBits, fractionalBits));

        public static Vector2 GetVector2(this PacketReader reader, int integerBits, int fractionalBits)
            => new Vector2(reader.GetFloat(integerBits, fractionalBits), reader.GetFloat(integerBits, fractionalBits));

        public static Vector3 GetVector3WithZAsFloat(this PacketReader reader, int integerBits = 14, int fractionalBits = 10)
            => new Vector3(
                reader.GetFloat(integerBits, fractionalBits), 
                reader.GetFloat(integerBits, fractionalBits), 
                reader.GetFloat());

        public static Vector3 GetNormalizedVector(this PacketReader reader)
        {
            bool yZero, zZero;
            bool xNegative;

            float x;
            float y;
            float z;

            xNegative = reader.GetBit();
            yZero = reader.GetBit();
            if (yZero)
            {
                y = 0;
            }
            else
            {
                y = reader.GetCompressedFloat();
            }

            zZero = reader.GetBit();
            if (zZero)
            {
                z = 0;
            }
            else
            {
                z = reader.GetCompressedFloat();
            }

            x = 1.0f - y * y - z * z;
            if (x < 0)
            {
                x = 0;
            }
            else
            {
                x = MathF.Sqrt(x);
            }

            if (xNegative)
            {
                x = -x;
            }

            return new Vector3(x, y, z);
        }

        public static Vector3 GetVelocityVector(this PacketReader reader)
        {
            if (!reader.GetBit())
            {
                return Vector3.Zero;
            }

            float length = reader.GetFloat();
            Vector3 normalizedVector = reader.GetNormalizedVector();
            return normalizedVector * length;
        }
    }
}
