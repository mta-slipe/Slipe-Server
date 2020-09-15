using FluentAssertions;
using MtaServer.Packets.Builder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace MtaServer.Packets.Tests
{
    public class PacketBuilderTests
    {

        [Fact]
        public void WriteIntTest()
        {
            var builder = new PacketBuilder();
            builder.Write(4096);

            var bytes = builder.Build();

            bytes.Should().Equal(new byte[]
            {
                0, 0x10, 0, 0
            });
        }

        [Theory]
        [InlineData(0.50f, new byte[] { 0b00000000, 0b00000000, 0b00000000, 0b00111111 })]
        [InlineData(128.56f, new byte[] { 0b01011100, 0b10001111, 0b00000000, 0b01000011 })]
        public void WriteFloatTest(float input, byte[] expectedOutput)
        {
            var builder = new PacketBuilder();
            builder.Write(input);

            var bytes = builder.Build();

            bytes.Should().Equal(expectedOutput);
        }

        [Theory]
        [InlineData(0x00, new byte[] { 0xf0 })]
        [InlineData(0x10, new byte[] { 0xe1, 0x00 })]
        [InlineData(0x1000, new byte[] { 0xC0, 0x02, 0x00 })]
        [InlineData(0x100000, new byte[] { 0x80, 0x00, 0x04, 0x00 })]
        [InlineData(0x10000000, new byte[] { 0x00, 0x00, 0x00, 0x08, 0x00 })]
        [InlineData(0xfedcbafe, new byte[] { 0x7F, 0x5D, 0x6E, 0x7F, 0x00 })]
        [InlineData(0xfedcbaab, new byte[] { 0x55, 0xDD, 0x6E, 0x7F, 0x00 })]
        public void WriteCompressedUintTest(uint value, byte[] expectedOutput)
        {
            var builder = new PacketBuilder();
            builder.WriteCompressed(value);

            var bytes = builder.Build();

            bytes.Should().Equal(expectedOutput);
        }

        [Theory]
        [InlineData(1235, new byte[] { 0b11011010, 0b01100000, 0b10000000 })]
        [InlineData(0, new byte[] { 0b11110000 })]
        public void WriteCompressedUlongTest(ulong value, byte[] expectedOutput)
        {
            var builder = new PacketBuilder();
            builder.WriteCompressed(value);

            var bytes = builder.Build();

            bytes.Should().Equal(expectedOutput);
        }

        [Theory]
        [InlineData(9, new byte[] { 0b111001_00 })]
        public void WriteCompressedUshortTest(ushort value, byte[] expectedOutput)
        {
            var builder = new PacketBuilder();
            builder.WriteCompressed(value);

            var bytes = builder.Build();

            bytes.Should().Equal(expectedOutput);
        }

        [Theory]
        [InlineData(0.56f, new byte[] { 0b10101101, 0b11000111 })]
        public void WriteCompressedFloatTest(float value, byte[] expectedOutput)
        {
            var builder = new PacketBuilder();
            builder.WriteCompressed(value);

            var bytes = builder.Build();

            bytes.Should().Equal(expectedOutput);
        }

        [Theory]
        [InlineData(new byte[] { 0b10100011 }, 4, new byte[] { 0b00110000 })]
        [InlineData(new byte[] { 0b11111111, 0b10100011 }, 12, new byte[] { 0b11111111, 0b00110000 })]
        public void WriteCappedTest(byte[] input, int bits, byte[] expectedOutput)
        {
            var builder = new PacketBuilder();

            builder.WriteBytesCapped(input, bits);
            builder.Length.Should().Be(bits);

            var bytes = builder.Build();
            bytes.Should().Equal(expectedOutput);
        }

        [Theory]
        [InlineData(0xCAF, 12, new byte[] { 0b10101111, 0b11000000 })]
        [InlineData(0xe38, 12, new byte[] { 0b00111000, 0b11100000 })]
        public void WriteCappedUint16Test(ushort input, int bits, byte[] expectedOutput)
        {
            var builder = new PacketBuilder();

            builder.WriteCapped(input, bits);
            builder.Length.Should().Be(bits);

            var bytes = builder.Build();
            bytes.Should().Equal(expectedOutput);
        }

        [Theory]
        [InlineData(0.5, 12, -3.14159f, 3.14159f, false, new byte[] { 0b01000101, 0b10010000 })]
        [InlineData(0.5, 16, -3.14159f, 3.14159f, false, new byte[] { 0b01011111, 0b10010100 })]
        [InlineData(128.573, 16, 0, 360, false, new byte[] { 0b01101110, 0b01011011 })]
        public void WriteFloatFromBitsTest(float input, int bitCount, float min, float max, bool preserve, byte[] expectedOutput)
        {
            var builder = new PacketBuilder();

            builder.WriteFloatFromBits(input, bitCount, min, max, preserve);

            var bytes = builder.Build();
            bytes.Should().Equal(expectedOutput);
        }

        [Theory]
        [InlineData(0.5, 0.5, 0.5, new byte[] { 0b00111111, 0b11101111, 0b11011111, 0b11110111, 0b11100000 })]
        public void WriteNormalizedVectorTest(float x, float y, float z, byte[] expectedOutput)
        {
            var builder = new PacketBuilder();

            builder.WriteNormalizedVector(new Vector3(x, y, z));

            var bytes = builder.Build();
            bytes.Should().Equal(expectedOutput);
        }

        [Theory]
        [InlineData(255, 255, 255, 255, false, false, new byte[] { 0xFF, 0xFF, 0xFF })]
        [InlineData(0, 255, 255, 255, false, false, new byte[] { 0xFF, 0xFF, 0xFF })]
        [InlineData(128, 255, 255, 255, true, true, new byte[] { 0x80, 0xFF, 0xFF, 0xFF })]
        [InlineData(128, 255, 255, 255, true, false, new byte[] { 0xFF, 0xFF, 0xFF, 0x80 })]
        public void WriteColorTest(byte alpha, byte red, byte green, byte blue, bool withAlpha, bool alphaFirst, byte[] expectedOutput)
        {
            var builder = new PacketBuilder();
            Color color = Color.FromArgb(alpha, red, green, blue);

            builder.Write(color, withAlpha, alphaFirst);

            var bytes = builder.Build();
            bytes.Should().Equal(expectedOutput);
        }

        [Fact]
        public void AlignToByteBoundaryTest()
        {
            var builder = new PacketBuilder();

            builder.Write(true);
            builder.AlignToByteBoundary();
            builder.Write(true);
            builder.AlignToByteBoundary();
            builder.Write(true);

            var bytes = builder.Build();
            bytes.Length.Should().Be(3);
            bytes.Should().Equal(new byte[] { 0b10000000, 0b10000000, 0b10000000 });
        }


    }
}
