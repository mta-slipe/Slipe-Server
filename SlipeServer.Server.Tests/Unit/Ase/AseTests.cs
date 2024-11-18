using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Ase;
public class AseTests
{
    [Fact]
    public void AsePingStatusShouldMatch()
    {
        var count = 1337;

        var bytes = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes((ushort)0x728D))
            .Concat(BitConverter.GetBytes((ushort)count))
            .Concat(BitConverter.GetBytes((ushort)0xFFFF));

        bytes.Should().BeEquivalentTo(new byte[]
        {
            0x8D, 0x72, 0x39, 0x05, 0xff, 0xff
        });
    }
}
