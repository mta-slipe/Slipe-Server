using FluentAssertions;
using MtaServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MtaServer.Packets.Tests
{
    public class PacketReaderTests
    {

        [Fact]
        public void WriteIntTest()
        {
            var reader = new PacketReader(new byte[]
            {
                0, 0x10, 0, 0
            });

            var value = reader.GetInt16();

            value.Should().Be(4096);
        }

        [Theory]
        [InlineData(0x00, new byte[] { 0xf0 })]
        [InlineData(0x10, new byte[] { 0xe1, 0x00 })]
        [InlineData(0x1000, new byte[] { 0xC0, 0x02, 0x00 })]
        [InlineData(0x100000, new byte[] { 0x80, 0x00, 0x04, 0x00 })]
        [InlineData(0x10000000, new byte[] { 0x00, 0x00, 0x00, 0x08, 0x00 })]
        [InlineData(0xfedcbafe, new byte[] { 0x7F, 0x5D, 0x6E, 0x7F, 0x00 })]
        [InlineData(0xfedcbaab, new byte[] { 0x55, 0xDD, 0x6E, 0x7F, 0x00 })]
        public void ReadCompressedTest(uint expectedValue, byte[] compressed)
        {
            var reader = new PacketReader(compressed);

            var value = reader.GetCompressedUInt32();

            value.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(new byte[] { 156 }, 8, 156)]
        [InlineData(new byte[] { 0b11111111 }, 4, 0b00001111)]
        [InlineData(new byte[] { 0b00001111 }, 4, 0b00000000)]
        [InlineData(new byte[] { 0b11110000 }, 4, 0b00001111)]
        [InlineData(new byte[] { 0b10010000 }, 4, 0b00001001)]
        [InlineData(new byte[] { 0b11000000 }, 4, 0b00001100)]
        public void ReadByteCappedTest(byte[] input, int bitCount, byte expectedOutput)
        {
            var reader = new PacketReader(input);

            var value = reader.GetByteCapped(bitCount);

            value.Should().Be(expectedOutput);
        }

        [Theory]
        [InlineData(new byte[] { 0b11000011 }, 4, new byte[] { 0b00001100 })]
        [InlineData(new byte[] { 0b11111111, 0b00110000 }, 12, new byte[] { 0b11111111, 0b00000011 })]
        [InlineData(new byte[] { 0b11111111, 0b00001111 }, 12, new byte[] { 0b11111111, 0b00000000 })]
        public void ReadBytesCappedTest(byte[] input, int bits, byte[] output)
        {
            var reader = new PacketReader(input);

            var value = reader.GetBytesCapped(bits);

            value.Should().Equal(output);
        }

        [Theory]
        [InlineData(0xCAF, 12, new byte[] { 0b10101111, 0b11000000 })]
        [InlineData(0xE38, 12, new byte[] { 0b00111000, 0b11100000 })]
        public void ReadCappedUint16Test(ushort expectedOutput, int bits, byte[] input)
        {
            var reader = new PacketReader(input);

            var bytes = reader.GetBytesCapped(bits);
            var uint16 = BitConverter.ToUInt16(bytes);

            uint16.Should().Be(expectedOutput);
        }

        [Fact]
        public void ReadBitsTest()
        {
            var reader = new PacketReader(new byte[] { 0b11001010 });

            var value = reader.GetBits(8);

            value.Should().Equal(new bool[]
            {
                true, true, false, false, true, false, true, false
            });
        }

        [Theory]
        [InlineData(new byte[] { 0b10101101, 0b11000111 }, 0.56f)]
        public void ReadCompressedFloatTest(byte[] input, float expectedOutput)
        {
            var reader = new PacketReader(input);

            var value = reader.GetCompressedFloat();

            value.Should().BeInRange(expectedOutput - 0.01f, expectedOutput + 0.01f);
        }
    }
}
