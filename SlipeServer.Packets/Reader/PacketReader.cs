using SlipeServer.Packets.Structs;
using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Reader;

public class PacketReader
{
    private readonly byte[] data;
    private int dataIndex;
    private byte byteIndex;

    public int Counter { get; private set; }
    public int Size { get; private set; }
    public bool IsFinishedReading => this.Counter == this.Size;

    public PacketReader(byte[] data)
    {
        this.data = data;
        this.Counter = 0;
        this.Size = data.Length * 8;

        this.dataIndex = 0;
        this.byteIndex = 128;
    }

    private bool GetBitFromData()
    {
        var current = this.data[this.dataIndex];
        var bit = (current & this.byteIndex) > 0;
        this.byteIndex >>= 1;
        if (this.byteIndex == 0)
        {
            this.byteIndex = 128;
            this.dataIndex++;
        }
        this.Counter++;
        return bit;
    }

    private byte GetByteFromData()
    {
        if ((this.Counter % 8) == 0)
        {
            this.Counter += 8;
            return this.data[this.dataIndex++];
        }

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

    private byte[] GetBytesFromData(uint count)
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
    public byte[] GetBytes(uint count) => GetBytesFromData(count);

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
        return Encoding.UTF8.GetString(GetBytes(length));
    }

    public string GetStringCharacters(uint length)
    {
        return Encoding.UTF8.GetString(GetBytes(length));
    }

    public string GetString()
    {
        int length = GetUint16();
        return GetStringCharacters(length);
    }

    public ElementId GetElementId()
    {
        var id = BitConverter.ToUInt32(GetBytesCapped(17).Concat(new byte[] { 0 }).ToArray(), 0);

        uint maxValue = (1 << 17) - 1;
        if (id == maxValue)
        {
            return (ElementId)PacketConstants.InvalidElementId;
        }

        return (ElementId)id;
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
        this.byteIndex = 128;
        this.dataIndex++;
    }
}
