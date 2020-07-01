using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets
{
    public class PacketBuilder
    {
        private readonly BitArray bits;

        public int Length => bits.Length;

        public PacketBuilder()
        {
            this.bits = new BitArray(0);
        }


        public byte[] Build()
        {
            byte[] result = new byte[(int)Math.Ceiling(this.bits.Length / 8.0f)];
            this.bits.Length = result.Length * 8;

            for (int i = 0; i < result.Length; i++)
            {

                byte value = 0;

                if (this.bits[i * 8 + 0]) value += 128;
                if (this.bits[i * 8 + 1]) value += 64;
                if (this.bits[i * 8 + 2]) value += 32;
                if (this.bits[i * 8 + 3]) value += 16;
                if (this.bits[i * 8 + 4]) value += 8;
                if (this.bits[i * 8 + 5]) value += 4;
                if (this.bits[i * 8 + 6]) value += 2;
                if (this.bits[i * 8 + 7]) value += 1;

                result[i] = value;
            }

            return result;
        }

        private void WriteBit(bool bit)
        {
            this.bits[this.bits.Length++] = bit;
        }

        private void WriteBytes(byte[] bytes)
        {
            foreach (byte b in bytes)
            {
                WriteBit((b & 128) > 0);
                WriteBit((b & 64) > 0);
                WriteBit((b & 32) > 0);
                WriteBit((b & 16) > 0);
                WriteBit((b & 8) > 0);
                WriteBit((b & 4) > 0);
                WriteBit((b & 2) > 0);
                WriteBit((b & 1) > 0);
            }
        }

        private void WriteByte(byte b)
        {
            WriteBit((b & 128) > 0);
            WriteBit((b & 64) > 0);
            WriteBit((b & 32) > 0);
            WriteBit((b & 16) > 0);
            WriteBit((b & 8) > 0);
            WriteBit((b & 4) > 0);
            WriteBit((b & 2) > 0);
            WriteBit((b & 1) > 0);
        }

        private void WriteByteCapped(byte b, int bitCount)
        {
            WriteBit((b & 128) > 0); if (--bitCount == 0) return;
            WriteBit((b & 64) > 0); if (--bitCount == 0) return;
            WriteBit((b & 32) > 0); if (--bitCount == 0) return;
            WriteBit((b & 16) > 0); if (--bitCount == 0) return;
            WriteBit((b & 8) > 0); if (--bitCount == 0) return;
            WriteBit((b & 4) > 0); if (--bitCount == 0) return;
            WriteBit((b & 2) > 0); if (--bitCount == 0) return;
            WriteBit((b & 1) > 0); if (--bitCount == 0) return;
        }

        public void WriteBytesCapped(byte[] bytes, int bitCount)
        {
            int byteCounter = 0;
            while (bitCount > 0)
            {
                var value = bytes[byteCounter++];

                if (bitCount >= 8)
                {
                    WriteByte(value);
                } else
                {
                    value <<= 8 - bitCount;

                    WriteByteCapped(value, bitCount);
                }

                bitCount -= 8;
            }
        }

        public void WriteCapped(int integer, int bitCap) => WriteBytesCapped(BitConverter.GetBytes(integer), bitCap);
        public void WriteCapped(short integer, int bitCap) => WriteBytesCapped(BitConverter.GetBytes(integer), bitCap);
        public void WriteCapped(long integer, int bitCap) => WriteBytesCapped(BitConverter.GetBytes(integer), bitCap);
        public void WriteCapped(uint integer, int bitCap) => WriteBytesCapped(BitConverter.GetBytes(integer), bitCap);
        public void WriteCapped(ushort integer, int bitCap) => WriteBytesCapped(BitConverter.GetBytes(integer), bitCap);
        public void WriteCapped(ulong integer, int bitCap) => WriteBytesCapped(BitConverter.GetBytes(integer), bitCap);
        public void WriteCapped(byte integer, int bitCap) => WriteBytesCapped(new byte[] { integer }, bitCap);


        public void WriteElementId(int integer) => WriteBytesCapped(BitConverter.GetBytes(integer), 17);
        public void WriteElementId(uint integer) => WriteBytesCapped(BitConverter.GetBytes(integer), 17);

        public void Write(float @float) => WriteBytes(BitConverter.GetBytes(@float));
        public void Write(int integer) => WriteBytes(BitConverter.GetBytes(integer));
        public void Write(short integer) => WriteBytes(BitConverter.GetBytes(integer));
        public void Write(long integer) => WriteBytes(BitConverter.GetBytes(integer));
        public void Write(uint integer) => WriteBytes(BitConverter.GetBytes(integer));
        public void Write(ushort integer) => WriteBytes(BitConverter.GetBytes(integer));
        public void Write(ulong integer) => WriteBytes(BitConverter.GetBytes(integer));
        public void Write(byte integer) => WriteBytes(new byte[] { integer });
        public void Write(byte[] bytes) => WriteBytes(bytes);

        public void Write(bool boolean) => WriteBit(boolean);

        public void Write(bool[] bits)
        {
            foreach (bool bit in bits)
            {
                WriteBit(bit);
            }
        }

        public void Write(string value)
        {
            Write((ushort)value.Length);
            WriteBytes(value.Select(c => (byte)c).ToArray());
        }

        public void WriteStringWithByteAsLength(string value)
        {
            Write((byte)value.Length);
            WriteBytes(value.Select(c => (byte)c).ToArray());
        }

        public void Write(Vector3 vector)
        {
            Write(vector.X);
            Write(vector.Y);
            Write(vector.Z);
        }

        public void Write(Vector2 vector)
        {
            Write(vector.X);
            Write(vector.Y);
        }

        public void Write(Color color, bool withAlpha = false)
        {
            Write((byte)color.R);
            Write((byte)color.G);
            Write((byte)color.B);
            if (withAlpha)
                Write((byte)color.A);
        }

        // The danger zone
        // from this point on you're going to see all kinds of weird MTA structures.

        private byte[] GetBytesFromInt(long value, int byteCount)
        {
            var intBytes = BitConverter.GetBytes(value);
            if (intBytes.Length == byteCount)
                return intBytes;

            byte[] bytes = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                bytes[i] = intBytes[i];
            }
            if (intBytes[intBytes.Length - 1] == 0xff && intBytes[intBytes.Length - 2] < 0x80)
            {
                bytes[bytes.Length - 1] &= 0x80;
            }

            return bytes;
        }

        public void WriteFloat(float value, int integerBits, int fractionalBits)
        {
            if ((integerBits + fractionalBits) % 8 != 0)
            {
                throw new NotSupportedException("WriteFloat does not support fractional bytes");
            }
            int integer = (int)(value * (1 << fractionalBits));
            byte[] bytes = GetBytesFromInt(integer, (integerBits + fractionalBits) / 8);
            Write(bytes);
        }

        public void Write(Vector3 vector, int integerBits, int fractionalBits)
        {
            WriteFloat(vector.X, integerBits, fractionalBits);
            WriteFloat(vector.Y, integerBits, fractionalBits);
            WriteFloat(vector.Z, integerBits, fractionalBits);
        }

        public void WriteVector2(Vector2 vector, int integerBits, int fractionalBits)
        {
            WriteFloat(vector.X, integerBits, fractionalBits);
            WriteFloat(vector.Y, integerBits, fractionalBits);
        }

        public void WriteVector3WithZAsFloat(Vector3 vector, int integerBits = 14, int fractionalBits = 10)
        {
            WriteFloat(vector.X, integerBits, fractionalBits);
            WriteFloat(vector.Y, integerBits, fractionalBits);
            Write(vector.Z);
        }

        float WrapAround(float low, float value, float high)
        {
            float size = high - low;
            return value - (size * MathF.Floor((value - low) / size));
        }

        float Unlerp(double min, double value, double max)
        {
            if (min == max)
                return 1.0f;

            return (float)((value - min) / (max - min));
        }

        public void WriteFloatFromBits(float value, int bitCount, float min, float max, bool preserveGreaterThanMin, bool wrap = false)
        {
            if (wrap)
            {
                value = WrapAround(min, value, max);
            }
            value = Math.Clamp(Unlerp(min, value, max), 0, 1);
            ulong integer = (ulong)Math.Round(((1 << bitCount) - 1) * value);

            if (preserveGreaterThanMin)
            {
                if (integer == 0 && value > 0.0f)
                {
                    integer = 1;
                }
            }
            WriteCapped((long)integer, bitCount);
        }

        public void WriteNormalizedVector(Vector3 vector)
        {
            float x = vector.X;
            float y = vector.Y;
            float z = vector.Z;

            Write(x < 0.0);
            if (y == 0.0)
                Write(true);
            else
            {
                Write(false);
                WriteCompressed(y);
            }
            if (z == 0.0)
                Write(true);
            else
            {
                Write(false);
                WriteCompressed(z);
            }
        }

        public void WriteVelocityVector(Vector3 vector)
        {
            Write(vector != Vector3.Zero);
            if (vector.Length() == 0)
                return;

            Write(vector.Length());
            WriteNormalizedVector(Vector3.Normalize(vector));
        }


        public void WriteCompressed(float value)
        {
            value = Math.Clamp(value, -1, 1);
            Write((ushort)((value + 1.0f) * 32767.5f));
        }


        // Reference source:
        // https://github.com/facebookarchive/RakNet/blob/1a169895a900c9fc4841c556e16514182b75faf8/Source/BitStream.cpp#L489
        public void WriteCompressed(byte[] inByteArray, bool unsignedData)
        {
            uint size = (uint)inByteArray.Length * 8;
            uint currentByte = (size >> 3) - 1; // PCs

            byte byteMatch;

            if (unsignedData)
            {
                byteMatch = 0;
            }
            else
            {
                byteMatch = 0xFF;
            }

            // Write upper bytes with a single 1
            // From high byte to low byte, if high byte is a byteMatch then write a 1 bit. Otherwise write a 0 bit and then write the remaining bytes
            while (currentByte > 0)
            {
                if (inByteArray[currentByte] == byteMatch)   // If high byte is byteMatch (0 of 0xff) then it would have the same value shifted
                {
                    Write(true);
                }
                else
                {
                    // Write the remainder of the data after writing 0
                    Write(false);

                    WriteBytesCapped(inByteArray, (int)((currentByte + 1) << 3));
                    //  currentByte--;

                    return;
                }

                currentByte--;
            }

            // If the upper half of the last byte is a 0 (positive) or 16 (negative) then write a 1 and the remaining 4 bits.  Otherwise write a 0 and the 8 bites.
            if ((unsignedData && (inByteArray[currentByte] & 0xF0) == 0x00) ||
                (unsignedData == false && (inByteArray[currentByte] & 0xF0) == 0xF0))
            {
                Write(true);
                WriteCapped(inByteArray[currentByte], 4);
            }

            else
            {
                Write(false);
                WriteCapped(inByteArray[currentByte], 8);
            }
        }

        public void WriteCompressed(byte @byte) => WriteCompressed(new byte[] { @byte }, true);
        public void WriteCompressed(ushort integer) => WriteCompressed(BitConverter.GetBytes(integer), true);
        public void WriteCompressed(short integer) => WriteCompressed(BitConverter.GetBytes(integer), false);
        public void WriteCompressed(uint integer) => WriteCompressed(BitConverter.GetBytes(integer), true);
        public void WriteCompressed(int integer) => WriteCompressed(BitConverter.GetBytes(integer), false);
    }
}
