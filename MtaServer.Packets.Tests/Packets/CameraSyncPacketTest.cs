using FluentAssertions;
using MtaServer.Packets.Definitions.Sync;
using System;
using System.Numerics;
using Xunit;

namespace MtaServer.Packets.Tests.Packets
{
    public class CameraSyncPacketTest
    {
        private readonly byte[] testPacket = new byte[]
        {
            0, 128, 110, 109, 0, 56, 115, 0, 0, 23, 162, 0, 0, 0, 0, 0, 0, 0, 0, 26, 34, 0
        };

        [Fact]
        public void ReadPacket_ReadsValuesProperly()
        {
            var packet = new CameraSyncPacket();

            packet.Read(this.testPacket);

            packet.IsFixed.Should().BeTrue();
            packet.TargetId.Should().Be(0);
            packet.Position.Should().Be(new Vector3(-2377, -1636, 700));
            packet.LookAt.Should().Be(new Vector3(0, 0, 720));
        }

        [Fact]
        public void WritePacket_MatchesExpectedByteArray()
        {
            var packet = new CameraSyncPacket(0, new Vector3(-2377, -1636, 700), new Vector3(0, 0, 720));

            var result = packet.Write();

            result.Should().Equal(this.testPacket);
        }
    }
}
