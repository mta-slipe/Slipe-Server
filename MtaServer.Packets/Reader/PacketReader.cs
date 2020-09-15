using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Reader
{
    public class PacketReader
    {
        private readonly BitArray bitArray;

        public int Counter { get; private set; }
        public int Size { get; private set; }
        public bool IsFinishedReading => Counter == bitArray.Length;

        public PacketReader(byte[] data)
        {
            this.bitArray = new BitArray(data);
            this.Counter = 0;
            this.Size = data.Length * 8;

            this.FlipBytes();
        }

        private void Swap(int indexA, int indexB)
        {
            bool temp = this.bitArray[indexA];
            bitArray[indexA] = this.bitArray[indexB];
            bitArray[indexB] = temp;
        }

        private void FlipBytes()
        {
            int byteCount = this.bitArray.Count / 8;
            for (int i = 0; i < byteCount; i++)
            {
                Swap(i * 8 + 0, i * 8 + 7);
                Swap(i * 8 + 1, i * 8 + 6);
                Swap(i * 8 + 2, i * 8 + 5);
                Swap(i * 8 + 3, i * 8 + 4);
            }
        }

        private bool GetBitFromData() => this.bitArray[this.Counter++];

        private byte GetByteFromData()
        {
            byte value = 0;

            if (GetBitFromData()) value += 128;
            if (GetBitFromData()) value += 64;
            if (GetBitFromData()) value += 32;
            if (GetBitFromData()) value += 16;
            if (GetBitFromData()) value += 8;
            if (GetBitFromData()) value += 4;
            if (GetBitFromData()) value += 2;
            if (GetBitFromData()) value += 1;

            return value;
        }

        private byte[] GetBytesFromData(int count)
        {
            byte[] bytes = new byte[count];
            for (int i = 0; i < count; i++)
            {
                bytes[i] = GetByteFromData();
            }
            return bytes;
        }

        public bool GetBit() => GetBitFromData();
        public bool[] GetBits(int count)
        {
            bool[] results = new bool[count];
            for (int i = 0; i < count; i++)
            {
                results[i] = GetBitFromData();
            }
            return results;
        }
        public byte GetByte() => GetByteFromData();
        public ushort GetUint16() => BitConverter.ToUInt16(GetBytes(2), 0);
        public uint GetUint32() => BitConverter.ToUInt32(GetBytes(4), 0);
        public ulong GetUint64() => BitConverter.ToUInt64(GetBytes(8), 0);
        public short GetInt16() => BitConverter.ToInt16(GetBytes(2), 0);
        public int GetInt32() => BitConverter.ToInt32(GetBytes(4), 0);
        public long GetInt64() => BitConverter.ToInt64(GetBytes(8), 0);
        public float GetFloat() => BitConverter.ToSingle(GetBytes(4), 0);
        public double GetDouble() => BitConverter.ToDouble(GetBytes(8), 0);
        public byte[] GetBytes(int count) => GetBytesFromData(count);

        public byte GetByteCapped(int bitCount, bool alignment = false)
        {
            byte value = 0;
            for (int i = bitCount - 1; i >= 0; i--)
            {
                if (GetBit())
                    value += (byte)(1 << (i));
            }
            return value;
        }

        public byte[] GetBytesCapped(int bitCount, bool alignment = false)
        {
            int byteCount = (int)Math.Ceiling(bitCount / 8d);
            byte[] values = new byte[byteCount];

            for (int i = 0; i < byteCount - 1; i++)
            {
                values[i] = GetByte();
            }

            var remainingBits = bitCount - (byteCount - 1) * 8;
            values[values.Length - 1] = GetByteCapped(remainingBits, alignment);

            return values;
        }

        public string GetStringCharacters(int length)
        {
            string result = "";
            for (int i = 0; i < length; i++)
            {
                result += (char)GetByteFromData();
            }
            return result;
        }

        public string GetStringCharacters(uint length)
        {
            string result = "";
            for (int i = 0; i < length; i++)
            {
                result += (char)GetByteFromData();
            }
            return result;
        }

        public string GetString()
        {
            int length = GetUint16();
            return GetStringCharacters(length);
        }

        public uint GetElementId()
        {
            var id = BitConverter.ToUInt32(GetBytesCapped(17).Concat(new byte[] { 0 }).ToArray(), 0);

            uint maxValue = (1 << 17) - 1;
            if (id == maxValue)
            {
                return PacketConstants.InvalidElementId;
            }

            return id;
        }


        // The danger zone
        // from this point on you're going to see all kinds of weird MTA structures.

        private long GetIntFromBytes(byte[] bytes)
        {
            int r = 0;
            byte b0 = 0xff;

            if ((bytes[bytes.Length - 1] & 0x80) != 0)
                r |= b0 << (bytes.Length * 8);

            for (int i = 0; i < bytes.Length; i++)
            {
                r |= bytes[bytes.Length - 1 - i] << ((bytes.Length - i - 1) * 8);
            }
            return r;
        }

        public ulong GetUIntFromBytes(byte[] bytes)
        {
            int r = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                r |= bytes[bytes.Length - 1 - i] << ((bytes.Length - i - 1) * 8);
            }
            return (ulong)r;
        }

        public float GetFloat(int integerBits, int fractionalBits)
        {
            byte[] integerBytes = GetBytesCapped(integerBits + fractionalBits, true);
            int intValue = (int)GetIntFromBytes(integerBytes);
            return (float)((double)intValue / (1 << fractionalBits));
        }

        public float GetFloatFromBits(int bitCount, float min, float max)
        {
            byte[] bytes;
            if (bitCount % 8 == 0)
            {
                bytes = GetBytes(bitCount / 8);
            } else
            {
                bytes = GetBytesCapped(bitCount);
            }
            ulong integer = GetUIntFromBytes(bytes);

            return min + integer / (float)((1 << bitCount) - 1) * (max - min);
        }

        public float GetFloatFromBits(uint bitCount, float min, float max) => GetFloatFromBits((int)bitCount, min, max);

        public void AlignToByteBoundary()
        {
            int bitsNeeded = 8 - (this.bitArray.Count % 8);
            if (bitsNeeded > 0 && bitsNeeded < 8)
            {
                GetBits(bitsNeeded);
            }
        }
    }
}
