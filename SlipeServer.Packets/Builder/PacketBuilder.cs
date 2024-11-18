﻿using SlipeServer.Packets.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SlipeServer.Packets.Builder;

public class PacketBuilder
{
    private readonly List<byte> data;
    private byte byteIndex;

    public int Length { get; private set; }

    public PacketBuilder()
    {
        this.data = [];
        this.byteIndex = 0;
    }


    public byte[] Build()
    {
        return this.data.ToArray();
    }

    private void WriteBit(bool bit)
    {
        if (this.byteIndex == 0)
        {
            this.byteIndex = 128;
            this.data.Add(0);
        }

        if (bit)
            this.data[this.data.Count - 1] += this.byteIndex;

        this.byteIndex >>= 1;
        this.Length++;
    }

    private void WriteBytes(IEnumerable<byte> bytes)
    {
        foreach (byte b in bytes)
            WriteByte(b);
    }

    private void WriteByte(byte b)
    {
        if (this.Length % 8 == 0)
        {
            this.data.Add(b);
            this.Length += 8;
            return;
        }

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
    public void WriteCapped(byte integer, int bitCap) => WriteBytesCapped([integer], bitCap);

    public void Write(ElementId id) => WriteElementId(id.Value);
    public void WriteElementId(uint integer) => WriteBytesCapped(BitConverter.GetBytes(integer), 17);

    public void Write(float @float) => WriteBytes(BitConverter.GetBytes(@float));
    public void Write(double value) => WriteBytes(BitConverter.GetBytes(value));
    public void Write(int integer) => WriteBytes(BitConverter.GetBytes(integer));
    public void Write(short integer) => WriteBytes(BitConverter.GetBytes(integer));
    public void Write(long integer) => WriteBytes(BitConverter.GetBytes(integer));
    public void Write(uint integer) => WriteBytes(BitConverter.GetBytes(integer));
    public void Write(ushort integer) => WriteBytes(BitConverter.GetBytes(integer));
    public void Write(ulong integer) => WriteBytes(BitConverter.GetBytes(integer));
    public void Write(byte integer) => WriteBytes([integer]);
    public void Write(IEnumerable<byte> bytes) => WriteBytes(bytes);

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
        var bytes = Encoding.UTF8.GetBytes(value);
        Write((ushort)bytes.Length);
        WriteBytes(bytes);
    }

    public void WriteStringWithoutLength(string value)
    {
        WriteBytes(Encoding.UTF8.GetBytes(value));
    }

    public void WriteStringWithoutLength(string value, int maxBytes)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteBytes(bytes.Take(maxBytes));
    }

    public void WriteStringWithByteAsLength(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Write((byte)bytes.Length);
        WriteBytes(bytes);
    }

    public void Write(Color color, bool withAlpha = false, bool alphaFirst = false)
    {
        if (withAlpha && alphaFirst)
            Write(color.A);

        Write(color.R);
        Write(color.G);
        Write(color.B);

        if (withAlpha && !alphaFirst)
            Write(color.A);
    }

    public void WriteBgra(Color color)
    {
        Write(color.B);
        Write(color.G);
        Write(color.R);
        Write(color.A);
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

    private float WrapAround(float low, float value, float high)
    {
        float size = high - low;
        return value - (size * MathF.Floor((value - low) / size));
    }

    private float Unlerp(double min, double value, double max)
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
        this.data.Add(0);
        this.byteIndex = 128;
    }

    public void WriteRange(ushort value, int bits, ushort min, ushort max)
    {
        value = (ushort)(Math.Clamp(value, min, max) - min);
        WriteCapped(value, bits);
    }

    public void WriteRange(short value, int bits, short min, short max)
    {
        value = (short)(Math.Clamp(value, min, max) - min);
        WriteCapped(value, bits);
    }
}
