using FluentAssertions;
using SlipeServer.Packets.Definitions.Sync;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Numerics;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets
{
    public class JoinCompletePacketTest
    {
        private readonly byte[] testPacket = new byte[]
        {
            9, 0, 67, 35, 32, 83, 101, 114, 118, 101, 114, 12, 0, 49, 46, 53, 46, 55, 45, 48, 49, 46, 48, 46, 48
        };

        [Fact]
        public void WritePacket_MatchesExpectedByteArray()
        {
            var packet = new JoinCompletePacket("C# Server", "1.5.7-01.0.0");

            var result = packet.Write();

            result.Should().Equal(this.testPacket);
        }

        private readonly byte[] testPacketTwo = new byte[]
        {
            0b00011100, 0b00000000, 0b01010011, 0b01101100, 0b01101001, 0b01110000, 0b01100101, 0b00100000, 0b01010011, 0b01100101, 0b01110010, 0b01110110, 0b01100101, 0b01110010, 0b00100000, 0b00110000, 0b00101110, 0b00110001, 0b00101110, 0b00110000, 0b00100000, 0b01011011, 0b01010111, 0b01101001, 0b01101110, 0b01100100, 0b01101111, 0b01110111, 0b01110011, 0b01011101, 0b00001011, 0b00000000, 0b00110001, 0b00101110, 0b00110101, 0b00101110, 0b00111000, 0b00101101, 0b00111001, 0b00101110, 0b00110000, 0b00101110, 0b00110000
        };

        [Fact]
        public void WritePacketTwo_MatchesExpectedByteArray()
        {
            var packet = new JoinCompletePacket("Slipe Server 0.1.0 [Windows]", "1.5.8-9.0.0");

            var result = packet.Write();

            result.Should().Equal(this.testPacketTwo);
        }
    }
}
