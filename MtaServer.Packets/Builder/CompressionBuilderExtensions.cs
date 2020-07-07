using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Builder
{
    public static class CompressionBuilderExtensions
    {
        public static void WriteCompressed(this PacketBuilder builder, float value)
        {
            value = Math.Clamp(value, -1, 1);
            builder.Write((ushort)((value + 1.0f) * 32767.5f));
        }


        // Reference source:
        // https://github.com/facebookarchive/RakNet/blob/1a169895a900c9fc4841c556e16514182b75faf8/Source/BitStream.cpp#L489
        public static void WriteCompressed(this PacketBuilder builder, byte[] inByteArray, bool unsignedData)
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
                    builder.Write(true);
                }
                else
                {
                    // Write the remainder of the data after writing 0
                    builder.Write(false);

                    builder.WriteBytesCapped(inByteArray, (int)((currentByte + 1) << 3));
                    //  currentByte--;

                    return;
                }

                currentByte--;
            }

            // If the upper half of the last byte is a 0 (positive) or 16 (negative) then write a 1 and the remaining 4 bits.  Otherwise write a 0 and the 8 bites.
            if ((unsignedData && (inByteArray[currentByte] & 0xF0) == 0x00) ||
                (unsignedData == false && (inByteArray[currentByte] & 0xF0) == 0xF0))
            {
                builder.Write(true);
                builder.WriteCapped(inByteArray[currentByte], 4);
            }

            else
            {
                builder.Write(false);
                builder.WriteCapped(inByteArray[currentByte], 8);
            }
        }

        public static void WriteCompressed(this PacketBuilder builder, byte @byte) => builder.WriteCompressed(new byte[] { @byte }, true);
        public static void WriteCompressed(this PacketBuilder builder, ushort integer) => builder.WriteCompressed(BitConverter.GetBytes(integer), true);
        public static void WriteCompressed(this PacketBuilder builder, short integer) => builder.WriteCompressed(BitConverter.GetBytes(integer), false);
        public static void WriteCompressed(this PacketBuilder builder, uint integer) => builder.WriteCompressed(BitConverter.GetBytes(integer), true);
        public static void WriteCompressed(this PacketBuilder builder, int integer) => builder.WriteCompressed(BitConverter.GetBytes(integer), false);

        // Turns out longs are just written as ints
        public static void WriteCompressed(this PacketBuilder builder, long integer) => builder.WriteCompressed(BitConverter.GetBytes((int)integer), false);
        public static void WriteCompressed(this PacketBuilder builder, ulong integer) => builder.WriteCompressed(BitConverter.GetBytes((uint)integer), true);
    }
}
