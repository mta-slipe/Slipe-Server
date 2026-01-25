using FluentAssertions;
using MTAServerWrapper.Packets.Outgoing.Connection;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets;

public class JoinCompletePacketTest
{
    private readonly byte[] testPacket = new byte[]
    {
        9, 0, 67, 35, 32, 83, 101, 114, 118, 101, 114, 12, 0, 49, 46, 54, 46, 48, 45, 48, 49, 46, 48, 46, 48
    };

    [Fact]
    public void WritePacket_MatchesExpectedByteArray()
    {
        var packet = new JoinCompletePacket("C# Server", "1.7.0-01.0.0");

        var result = packet.Write();

        result.Should().Equal(this.testPacket);
    }

    private readonly byte[] testPacket2 = new byte[]
    {
        29, 0, 83, 108, 105, 112, 101, 32, 83, 101, 114, 118, 101, 114, 32, 48, 46, 49, 46, 48, 32, 91, 87, 105, 110, 100, 111, 119, 115, 93, 0, 11, 0, 49, 46, 54, 46, 48, 45, 57, 46, 48, 46, 48
    };

    [Fact]
    public void WritePacket2_MatchesExpectedByteArray()
    {
        var packet = new JoinCompletePacket("Slipe Server 0.1.0 [Windows]\0", "1.7.0-9.0.0");

        var result = packet.Write();

        result.Should().Equal(this.testPacket2);
    }
}
