using FluentAssertions;
using MtaServer.Packets.Definitions.Sync;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Numerics;
using Xunit;

namespace MtaServer.Packets.Tests.Packets
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
    }
}
