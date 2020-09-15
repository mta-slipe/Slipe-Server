using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Builder
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

        public void WriteElementId(uint integer) => WriteBytesCapped(BitConverter.GetBytes(integer), 17);

        public void Write(float @float) => WriteBytes(BitConverter.GetBytes(@float));
        public void Write(double value) => WriteBytes(BitConverter.GetBytes(value));
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

        public void WriteStringWithoutLength(string value)
        {
            WriteBytes(value.Select(c => (byte)c).ToArray());
        }

        public void WriteStringWithByteAsLength(string value)
        {
            Write((byte)value.Length);
            WriteBytes(value.Select(c => (byte)c).ToArray());
        }

        public void Write(Color color, bool withAlpha = false, bool alphaFirst = false)
        {
            if (withAlpha && alphaFirst)
                Write((byte)color.A);

            Write((byte)color.R);
            Write((byte)color.G);
            Write((byte)color.B);

            if (withAlpha && !alphaFirst)
                Write((byte)color.A);
        }

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

        public void AlignToByteBoundary()
        {
            int bitsNeeded = 8 - (this.bits.Count % 8);
            if (bitsNeeded > 0 && bitsNeeded < 8)
            {
                for (int i = 0; i < bitsNeeded; i++)
                {
                    WriteBit(false);
                }
            }
        }
    }
}
