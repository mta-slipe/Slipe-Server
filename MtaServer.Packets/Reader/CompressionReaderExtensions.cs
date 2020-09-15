using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtaServer.Packets.Reader
{
    public static class CompressionReaderExtensions
    {
        // Reference source:
        // https://github.com/facebookarchive/RakNet/blob/1a169895a900c9fc4841c556e16514182b75faf8/Source/BitStream.cpp#L617
        public static byte[] GetCompressed(this PacketReader reader, uint size, bool unsignedData)
        {
            byte[] data = new byte[size / 8];
            uint currentByte = (size >> 3) - 1;
            byte byteMatch;
            byte halfByteMatch;

            if (unsignedData)
            {
                byteMatch = 0;
                halfByteMatch = 0;
            }
            else
            {
                byteMatch = 0xFF;
                halfByteMatch = 0xF0;
            }

            // Upper bytes are specified with a single 1 if they match byteMatch
            // From high byte to low byte, if high byte is a byteMatch then write a 1 bit. Otherwise write a 0 bit and then write the remaining bytes
            while (currentByte > 0)
            {
                // If we read a 1 then the data is byteMatch.

                bool isByteMatch = reader.GetBit();

                if (isByteMatch)   // Check that bit
                {
                    data[currentByte] = byteMatch;
                    currentByte--;
                }
                else
                {
                    // Read the rest of the bytes


                    var restData = reader.GetBytesCapped((int)((currentByte + 1) << 3), true);
                    return restData
                        .Take((int)currentByte + 1)
                        .Concat(data.Take((int)(data.Length - currentByte - 1)))
                        .ToArray();
                }
            }

            // All but the first bytes are byteMatch.  If the upper half of the last byte is a 0 (positive) or 16 (negative) then what we read will be a 1 and the remaining 4 bits.
            // Otherwise we read a 0 and the 8 bytes
            //RakAssert(readOffset+1 <=numberOfBitsUsed); // If this assert is hit the stream wasn't long enough to read from
            //if (readOffset + 1 > numberOfBitsUsed )
            // return data;

            bool b = reader.GetBit();

            if (b)   // Check that bit
            {
                data[currentByte] = reader.GetByteCapped(4);

                data[currentByte] |= halfByteMatch; // We have to set the high 4 bits since these are set to 0 by ReadBits
            }
            else
            {
                data[currentByte] = reader.GetByte();
            }

            return data;
        }

        public static byte GetCompressedByte(this PacketReader reader) => reader.GetCompressed(8, true)[0];

        public static ushort GetCompressedUint16(this PacketReader reader) => BitConverter.ToUInt16(reader.GetCompressed(16, true));
        public static short GetCompressedint16(this PacketReader reader) => BitConverter.ToInt16(reader.GetCompressed(16, true));

        public static uint GetCompressedUInt32(this PacketReader reader) => BitConverter.ToUInt32(reader.GetCompressed(32, true));
        public static int GetCompressedInt32(this PacketReader reader) => BitConverter.ToInt32(reader.GetCompressed(32, true));

        public static ulong GetCompressedUInt64(this PacketReader reader) => BitConverter.ToUInt64(reader.GetCompressed(64, true));
        public static long GetCompressedInt64(this PacketReader reader) => BitConverter.ToInt64(reader.GetCompressed(64, true));


        public static float GetCompressedFloat(this PacketReader reader)
        {
            return reader.GetUint16() / 32767.5f - 1.0f;
        }
    }
}
