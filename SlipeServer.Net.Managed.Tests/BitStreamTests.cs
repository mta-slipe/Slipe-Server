using SlipeServer.Net.Managed;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Static self-checks for the <c>BitStream</c> port (findings/bitstream-wire-format.md §12).
/// These prove internal consistency of our reader/writer pair. They do NOT prove wire
/// compatibility with a real client — only a real client can do that (poc-plan.md §22.7).
/// </summary>
public class BitStreamTests
{
    [Fact]
    public void BitRoundTrip_IsMsbFirst()
    {
        var bs = new BitStream();
        bs.WriteBit(true);
        bs.WriteBit(false);
        bs.WriteBit(true);
        bs.WriteBit(true);

        // 1011 -> 0xB0 in the first byte, high nibble first.
        Assert.Equal(0xB0, bs.ToArray()[0]);

        Assert.True(bs.ReadBit());
        Assert.False(bs.ReadBit());
        Assert.True(bs.ReadBit());
        Assert.True(bs.ReadBit());
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(3u)]
    [InlineData(4u)]
    [InlineData(5u)]
    [InlineData(7u)]
    public void ThreeBitValue_RoundTripsRightAligned(uint value)
    {
        var bs = new BitStream();
        bs.WriteBits(new[] { (byte)value }, 3, true);
        Assert.Equal(3, bs.NumberOfBitsUsed);

        var back = new byte[1];
        Assert.True(bs.ReadBits(back, 3, true));
        Assert.Equal((byte)value, back[0]);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(17u)]
    [InlineData(31u)]
    public void FiveBitValue_RoundTripsRightAligned(uint value)
    {
        var bs = new BitStream();
        bs.WriteBits(new[] { (byte)value }, 5, true);

        var back = new byte[1];
        Assert.True(bs.ReadBits(back, 5, true));
        Assert.Equal((byte)value, back[0]);
    }

    [Fact]
    public void LittleEndianPrimitives_RoundTrip()
    {
        var bs = new BitStream();
        bs.Write((byte)0x5A);
        bs.Write((ushort)0x1234);
        bs.Write(0xDEADBEEFu);
        bs.Write(0x0123456789ABCDEFul);

        // Little-endian on the wire (§12.5).
        var raw = bs.ToArray();
        Assert.Equal(0x5A, raw[0]);
        Assert.Equal(0x34, raw[1]);
        Assert.Equal(0x12, raw[2]);
        Assert.Equal(0xEF, raw[3]);
        Assert.Equal(0xBE, raw[4]);
        Assert.Equal(0xAD, raw[5]);
        Assert.Equal(0xDE, raw[6]);

        Assert.True(bs.Read(out byte b));
        Assert.True(bs.Read(out ushort u16));
        Assert.True(bs.Read(out uint u32));
        Assert.True(bs.Read(out ulong u64));
        Assert.Equal(0x5A, b);
        Assert.Equal(0x1234, u16);
        Assert.Equal(0xDEADBEEFu, u32);
        Assert.Equal(0x0123456789ABCDEFul, u64);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(0x0F)]
    [InlineData(0x10)]
    [InlineData(0x58)]
    [InlineData(0x7F)]
    [InlineData(0xFF)]
    [InlineData(0x100)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteCompressedUshort_RoundTrips(int value)
    {
        var bs = new BitStream();
        bs.WriteCompressed((ushort)value);

        Assert.True(bs.ReadCompressed(out ushort back));
        Assert.Equal((ushort)value, back);
        Assert.Equal(bs.NumberOfBitsUsed, bs.ReadOffset);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(0xFFu)]
    [InlineData(0x100u)]
    [InlineData(0xFFFFu)]
    [InlineData(0x10000u)]
    [InlineData(0xDEADBEEFu)]
    public void WriteCompressedUint_RoundTrips(uint value)
    {
        var bs = new BitStream();
        bs.WriteCompressed(value);

        Assert.True(bs.ReadCompressed(out uint back));
        Assert.Equal(value, back);
        Assert.Equal(bs.NumberOfBitsUsed, bs.ReadOffset);
    }

    [Fact]
    public void WriteCompressed_MatchesDocumentedShapes()
    {
        // 0x58 (the 88-bit payload count the loopback uses): high nibble non-zero, low byte
        // alone, so it costs one "not compressed" bit at position 1, one terminal bit, 8 bits.
        var a = new BitStream();
        a.WriteCompressed((ushort)0x58);
        Assert.Equal(10, a.NumberOfBitsUsed);

        // A value whose high byte is zero and whose low nibble is clear costs 1 + 1 + 4 = 6 bits.
        var b = new BitStream();
        b.WriteCompressed((ushort)0x01);
        Assert.Equal(6, b.NumberOfBitsUsed);

        // 0x0100 has a non-zero high byte, so it falls back to the full 16-bit form: 1 + 16.
        var c = new BitStream();
        c.WriteCompressed((ushort)0x0100);
        Assert.Equal(17, c.NumberOfBitsUsed);
    }

    [Fact]
    public void WriteAlignedBytes_RoundsUpAndRoundTrips()
    {
        var bs = new BitStream();
        bs.WriteBits(new byte[] { 0x05 }, 3, true);   // 3 bits written
        bs.WriteAlignedBytes(new byte[] { 0xAA, 0xBB, 0xCC }, 3);

        // 3 bits padded to 8, then 24 payload bits.
        Assert.Equal(32, bs.NumberOfBitsUsed);
        var raw = bs.ToArray();
        Assert.Equal(0xAA, raw[1]);
        Assert.Equal(0xBB, raw[2]);
        Assert.Equal(0xCC, raw[3]);

        var dst = new byte[3];
        bs.IgnoreBits(3);                             // the read side must skip the same 3 bits
        Assert.True(bs.ReadAlignedBytes(dst, 3));
        Assert.Equal(new byte[] { 0xAA, 0xBB, 0xCC }, dst);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(127)]
    [InlineData(128)]
    [InlineData(0x7EFF)]
    [InlineData(0x7F00)]
    [InlineData(0x123456)]
    public void LengthLadder_RoundTrips(int value)
    {
        // §12.1: <= 0x7F -> 1 byte; <= 0x7EFF -> 2 bytes; else 0xFF + uint32.
        var bs = new BitStream();
        bs.WriteLength(value);

        int expectedBytes = value <= 0x7F ? 1 : value <= 0x7EFF ? 2 : 5;
        Assert.Equal(expectedBytes * 8, bs.NumberOfBitsUsed);

        Assert.True(bs.ReadLength(out int back));
        Assert.Equal(value, back);
    }

    [Fact]
    public void WriteString_RoundTripsWithUshortLength()
    {
        var bs = new BitStream();
        bs.WriteString("MtaNetPoc!");

        // 16-bit length prefix + 10 raw bytes.
        Assert.Equal(16 + 80, bs.NumberOfBitsUsed);

        Assert.True(bs.ReadString(out string back));
        Assert.Equal("MtaNetPoc!", back);
        Assert.True(bs.IsAtEnd);
    }

    [Fact]
    public void WriteStr_RoundTripsWithTheLengthLadder()
    {
        var bs = new BitStream();
        bs.WriteStr("MtaNetPoc!");

        Assert.True(bs.ReadStr(out string back));
        Assert.Equal("MtaNetPoc!", back);
        Assert.True(bs.IsAtEnd);
    }

    [Fact]
    public void ReadBits_RefusesToOverrun()
    {
        var bs = new BitStream();
        bs.Write((byte)0xFF);

        var dst = new byte[4];
        Assert.False(bs.ReadBits(dst, 16, true));
    }
}
