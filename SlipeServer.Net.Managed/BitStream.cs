using System.Text;

namespace SlipeServer.Net.Managed;

/// <summary>
/// Bit-exact RakNet <c>BitStream</c> as used on the MTA:SA wire.
/// </summary>
/// <remarks>
/// Bit fields are MSB-first; multi-byte typed values are little-endian (the stream runs in
/// native-endian mode on little-endian hosts).
/// </remarks>
public sealed class BitStream
{
    private byte[] _buffer;

    /// <summary>Number of bits written so far.</summary>
    private int _bitLength;

    /// <summary>Read cursor, in bits.</summary>
    private int _readOffset;

    /// <summary>Creates a writable stream.</summary>
    public BitStream(int initialBytes = 64)
    {
        _buffer = new byte[Math.Max(8, initialBytes)];
    }

    /// <summary>Creates a readable stream over a copy of <paramref name="data"/>.</summary>
    public BitStream(byte[] data, int offset, int byteCount)
    {
        _buffer = new byte[Math.Max(1, byteCount)];
        Array.Copy(data, offset, _buffer, 0, byteCount);
        _bitLength = byteCount * 8;
    }

    public static BitStream Read(byte[] data) => new(data, 0, data.Length);

    // ---------------------------------------------------------------- state

    public int NumberOfBitsUsed => _bitLength;

    public int ReadOffset => _readOffset;

    /// <summary>Bits left between the read cursor and the end of the data.</summary>
    public int GetNumberOfUnreadBits() => Math.Max(0, _bitLength - _readOffset);

    /// <summary>Written length rounded up to whole bytes.</summary>
    public int GetNumberOfBytesUsed() => (_bitLength + 7) >> 3;

    public bool IsAtEnd => _readOffset >= _bitLength;

    /// <summary>
    /// The number of whole bytes <see cref="ReadAlignedBytes"/> could still deliver: the read cursor
    /// rounded up to a byte boundary, subtracted from the bit length.
    ///
    /// <para>
    /// This exists so a parser can bounds-check a wire length field against the datagram <em>before</em>
    /// it sizes a buffer from it. <see cref="ReadAlignedBytes"/> rejects the overrun either way, so
    /// asking this first changes nothing about which datagrams are accepted, only how much memory a
    /// rejected one costs.
    /// </para>
    /// </summary>
    public int GetNumberOfAlignedBytesRemaining()
    {
        int aligned = RoundUpToByteBoundary(_readOffset);
        return aligned >= _bitLength ? 0 : (_bitLength - aligned) >> 3;
    }

    /// <summary>Raw view of the written bytes (rounded up to a byte boundary).</summary>
    public byte[] ToArray()
    {
        int n = GetNumberOfBytesUsed();
        var result = new byte[n];
        Array.Copy(_buffer, 0, result, 0, n);
        return result;
    }

    public byte[] Buffer => _buffer;

    // ------------------------------------------------------------- bit I/O

    public void WriteBit(bool value)
    {
        EnsureCapacity(_bitLength + 1);
        if (value)
        {
            _buffer[_bitLength >> 3] |= (byte)(0x80 >> (_bitLength & 7));
        }
        _bitLength++;
    }

    public bool ReadBit()
    {
        if (_readOffset + 1 > _bitLength)
        {
            return false;
        }
        bool value = (_buffer[_readOffset >> 3] & (0x80 >> (_readOffset & 7))) != 0;
        _readOffset++;
        return value;
    }

    /// <summary>
    /// Writes <paramref name="numBits"/> bits, most-significant first. When
    /// <paramref name="rightAligned"/> is set a partial final byte contributes its low bits.
    /// </summary>
    public void WriteBits(byte[] src, int numBits, bool rightAligned = true)
    {
        int remaining = numBits;
        int index = 0;
        while (remaining > 0)
        {
            int chunk = Math.Min(8, remaining);
            byte b = src[index];
            if (chunk < 8 && rightAligned)
            {
                b = (byte)(b << (8 - chunk));
            }
            for (int i = 0; i < chunk; i++)
            {
                WriteBit(((b >> (7 - i)) & 1) != 0);
            }
            remaining -= chunk;
            index++;
        }
    }

    /// <summary>
    /// Reads <paramref name="numBits"/> bits, most-significant first. The bounds check is an
    /// <em>unsigned</em> comparison; when <paramref name="rightAligned"/> is set a partial final byte
    /// is returned right-aligned in its destination byte.
    /// </summary>
    public bool ReadBits(byte[] dst, int numBits, bool rightAligned = true)
    {
        if (numBits == 0)
        {
            return true;
        }
        // A destination shorter than the field is a caller error; refuse rather than overrun. Every
        // call site sizes dst from numBits, so this only fires for hand-built input.
        if ((numBits + 7) >> 3 > dst.Length)
        {
            return false;
        }
        // Unsigned guard: (readOffset + numBits) must not wrap and must fit in bitsUsed.
        if (_readOffset + (uint)numBits > (uint)_bitLength)
        {
            return false;
        }

        Array.Clear(dst, 0, (numBits + 7) >> 3);

        int remaining = numBits;
        int index = 0;
        while (remaining > 0)
        {
            int chunk = Math.Min(8, remaining);
            int value = 0;
            for (int i = 0; i < chunk; i++)
            {
                value = (value << 1) | (ReadBit() ? 1 : 0);
            }
            dst[index] = chunk < 8 && rightAligned
                ? (byte)value
                : (byte)(value << (8 - chunk));
            remaining -= chunk;
            index++;
        }
        return true;
    }

    public void IgnoreBits(int numBits)
    {
        var scratch = new byte[(numBits + 7) >> 3];
        ReadBits(scratch, numBits, false);
    }

    // ------------------------------------------------------ aligned bytes

    /// <summary>Rounds the write cursor up to a byte boundary, then copies whole bytes.</summary>
    public void WriteAlignedBytes(byte[] src, int byteCount)
    {
        AlignWriteToByteBoundary();
        for (int i = 0; i < byteCount; i++)
        {
            WriteBits(new[] { src[i] }, 8, true);
        }
    }

    /// <summary>Rounds the read cursor up to a byte boundary, then copies whole bytes.</summary>
    public bool ReadAlignedBytes(byte[] dst, int byteCount)
    {
        AlignReadToByteBoundary();
        if (byteCount == 0)
        {
            return true;
        }
        if (_readOffset + byteCount * 8 > _bitLength)
        {
            return false;
        }
        Array.Copy(_buffer, _readOffset >> 3, dst, 0, byteCount);
        _readOffset += byteCount * 8;
        return true;
    }

    /// <summary>Rounds a bit offset up to a multiple of 8; zero stays zero.</summary>
    private static int RoundUpToByteBoundary(int offset) =>
        offset == 0 ? 0 : (offset - ((offset - 1) & 7)) + 7;

    public void AlignWriteToByteBoundary() => _bitLength = RoundUpToByteBoundary(_bitLength);

    public void AlignReadToByteBoundary() => _readOffset = RoundUpToByteBoundary(_readOffset);

    // --------------------------------------------------------- compressed

    /// <summary>
    /// Reads a RakNet-compressed value: each leading byte equal to the fill byte collapses to one set bit.
    /// </summary>
    /// <remarks>
    /// The terminal branch tests <em>the bit just read from the stream</em>, not
    /// <paramref name="rightAligned"/>. That parameter only picks the fill byte and the OR mask:
    /// <c>0x00</c>/<c>0x00</c> when set, <c>0xFF</c>/<c>0xF0</c> when clear. Every caller passes
    /// <c>rightAligned = true</c>, so the live case is fill <c>0x00</c>, mask <c>0x00</c>.
    /// </remarks>
    public bool ReadCompressed(byte[] dst, int numBits, bool rightAligned = true)
    {
        // As in ReadBits, the destination must hold the whole field before anything is written;
        // this method writes dst[i] for every i < numBits/8 as well as dst[0].
        if (Math.Max(1, (numBits + 7) >> 3) > dst.Length)
        {
            return false;
        }

        byte fillByte = rightAligned ? (byte)0x00 : (byte)0xFF;
        byte orMask = rightAligned ? (byte)0x00 : (byte)0xF0;

        int byteCount = numBits >> 3;
        Array.Clear(dst, 0, Math.Max(1, (numBits + 7) >> 3));

        for (int i = byteCount - 1; i > 0; i--)
        {
            if (_readOffset + 1 > _bitLength)
            {
                return false;
            }
            if (!ReadBit())
            {
                return ReadBits(dst, i * 8 + 8, true);
            }
            dst[i] = fillByte;
        }

        if (_readOffset + 1 > _bitLength)
        {
            return false;
        }
        if (!ReadBit())
        {
            return ReadBits(dst, 8, true);
        }
        if (!ReadBits(dst, 4, true))
        {
            return false;
        }
        dst[0] |= orMask;
        return true;
    }

    /// <summary>Exact mirror of <see cref="ReadCompressed"/> (RakNet <c>WriteCompressed</c>).</summary>
    public void WriteCompressed(byte[] src, int numBits, bool rightAligned = true)
    {
        byte fillByte = rightAligned ? (byte)0x00 : (byte)0xFF;
        byte maskNibble = rightAligned ? (byte)0x00 : (byte)0xF0;

        int byteCount = numBits >> 3;

        for (int i = byteCount - 1; i > 0; i--)
        {
            if (src[i] == fillByte)
            {
                WriteBit(true);
            }
            else
            {
                WriteBit(false);
                WriteBits(src, i * 8 + 8, true);
                return;
            }
        }

        // The reader does ReadBits(dst, 4, true) -> the nibble lands right-aligned in dst[0],
        // then ORs in the mask. So the wire carries the LOW nibble of src[0], and the mask
        // restores the high nibble. WriteBits shifts a partial chunk itself, so pass the raw
        // nibble with no pre-shift.
        if ((src[0] & 0xF0) == maskNibble)
        {
            WriteBit(true);
            WriteBits(new[] { (byte)(src[0] & 0x0F) }, 4, true);
        }
        else
        {
            WriteBit(false);
            WriteBits(src, 8, true);
        }
    }

    public bool ReadCompressed(out ushort value)
    {
        var b = new byte[2];
        if (!ReadCompressed(b, 16))
        {
            value = 0;
            return false;
        }
        value = (ushort)(b[0] | (b[1] << 8));
        return true;
    }

    public bool ReadCompressed(out uint value)
    {
        var b = new byte[4];
        if (!ReadCompressed(b, 32))
        {
            value = 0;
            return false;
        }
        value = (uint)b[0] | ((uint)b[1] << 8) | ((uint)b[2] << 16) | ((uint)b[3] << 24);
        return true;
    }

    public void WriteCompressed(ushort value) =>
        WriteCompressed(new[] { (byte)value, (byte)(value >> 8) }, 16);

    public void WriteCompressed(uint value) =>
        WriteCompressed(new[] { (byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) }, 32);

    // -------------------------------------------------- typed primitives

    public void Write(byte value) => WriteBits(new[] { value }, 8, true);

    public void Write(bool value) => WriteBit(value);

    public void Write(ushort value) => WriteBits(new[] { (byte)value, (byte)(value >> 8) }, 16, true);

    public void Write(uint value) =>
        WriteBits(new[] { (byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) }, 32, true);

    public void Write(ulong value)
    {
        var b = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            b[i] = (byte)(value >> (8 * i));
        }
        WriteBits(b, 64, true);
    }

    public void Write(byte[] bytes, int count = -1)
    {
        int n = count < 0 ? bytes.Length : count;
        WriteBits(bytes, n * 8, true);
    }

    /// <summary>RakNet <c>Write(const char*)</c>: <c>unsigned short</c> length prefix + raw characters.</summary>
    public void WriteString(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Write((ushort)bytes.Length);
        Write(bytes);
    }

    /// <summary>RakNet <c>WriteStr</c>: <c>WriteLength</c> prefix + raw characters.</summary>
    public void WriteStr(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteLength(bytes.Length);
        Write(bytes);
    }

    public bool Read(out byte value)
    {
        var b = new byte[1];
        if (!ReadBits(b, 8, true))
        {
            value = 0;
            return false;
        }
        value = b[0];
        return true;
    }

    public bool Read(out ushort value)
    {
        var b = new byte[2];
        if (!ReadBits(b, 16, true))
        {
            value = 0;
            return false;
        }
        value = (ushort)(b[0] | (b[1] << 8));
        return true;
    }

    public bool Read(out uint value)
    {
        var b = new byte[4];
        if (!ReadBits(b, 32, true))
        {
            value = 0;
            return false;
        }
        value = (uint)b[0] | ((uint)b[1] << 8) | ((uint)b[2] << 16) | ((uint)b[3] << 24);
        return true;
    }

    public bool Read(out ulong value)
    {
        var b = new byte[8];
        if (!ReadBits(b, 64, true))
        {
            value = 0;
            return false;
        }
        ulong v = 0;
        for (int i = 7; i >= 0; i--)
        {
            v = (v << 8) | b[i];
        }
        value = v;
        return true;
    }

    public bool ReadString(out string value)
    {
        value = string.Empty;
        if (!Read(out ushort length))
        {
            return false;
        }
        if (length == 0)
        {
            return true;
        }
        if (!this.LengthFitsRemainingBits(length))
        {
            return false;
        }
        var b = new byte[length];
        if (!ReadBits(b, length * 8, true))
        {
            return false;
        }
        value = Encoding.UTF8.GetString(b);
        return true;
    }

    /// <summary>RakNet <c>ReadStr</c>: the <c>ReadLength</c> mirror of <see cref="WriteStr"/>.</summary>
    public bool ReadStr(out string value)
    {
        value = string.Empty;
        if (!ReadLength(out int length))
        {
            return false;
        }
        if (length == 0)
        {
            return true;
        }
        if (!this.LengthFitsRemainingBits(length))
        {
            return false;
        }
        var b = new byte[length];
        if (!ReadBits(b, length * 8, true))
        {
            return false;
        }
        value = Encoding.UTF8.GetString(b);
        return true;
    }

    /// <summary>
    /// True when a length-prefixed field of <paramref name="length"/> bytes can be satisfied by the
    /// bits that remain. The <see cref="ReadLength"/> ladder encodes up to <c>0xFFFFFFFF</c> and
    /// <see cref="ReadLength"/> casts it to <see cref="int"/>, so a hostile stream can ask for a
    /// negative or four-gigabyte buffer; checking the length against the stream before allocating is
    /// the difference between a rejected datagram and an <see cref="OutOfMemoryException"/>.
    /// </summary>
    private bool LengthFitsRemainingBits(int length) =>
        length > 0 && (long)length * 8 <= this.GetNumberOfUnreadBits();

    /// <summary>
    /// RakNet <c>WriteLength</c> ladder: one byte up to 0x7F, two bytes (high byte + 128) up to 0x7EFF,
    /// otherwise 0xFF followed by a 32-bit value.
    /// </summary>
    public void WriteLength(int length)
    {
        if (length <= 0x7F)
        {
            Write((byte)length);
        }
        else if (length <= 0x7EFF)
        {
            Write((byte)((length >> 8) + 128));
            Write((byte)(length & 0xFF));
        }
        else
        {
            Write((byte)0xFF);
            Write((uint)length);
        }
    }

    public bool ReadLength(out int length)
    {
        length = 0;
        if (!Read(out byte b0))
        {
            return false;
        }
        if (b0 <= 0x7F)
        {
            length = b0;
            return true;
        }
        if (b0 != 0xFF)
        {
            if (!Read(out byte b1))
            {
                return false;
            }
            length = ((b0 - 128) << 8) + b1;
            return true;
        }
        if (!Read(out uint full))
        {
            return false;
        }
        length = (int)full;
        return true;
    }

    // ------------------------------------------------------------ capacity

    private void EnsureCapacity(int bits)
    {
        int needed = (bits + 7) >> 3;
        if (needed <= _buffer.Length)
        {
            return;
        }
        int size = _buffer.Length;
        while (size < needed)
        {
            size *= 2;
        }
        Array.Resize(ref _buffer, size);
    }
}
