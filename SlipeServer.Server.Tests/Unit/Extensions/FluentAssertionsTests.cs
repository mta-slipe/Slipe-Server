using SlipeServer.Server.Elements;
using SlipeServer.Server.TestTools.FluentAssertions;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Extensions;

public class FluentAssertionsTests
{
    [Fact]
    public void VehiclesShouldBeTheSame()
    {
        var a = new Vehicle(404, new Vector3(0, 0, 0));
        var b = new Vehicle(404, new Vector3(0, 0, 0));
        a.Should().BeEquivalentTo(b);
    }

    [Fact]
    public void PlayersShouldBeTheSame()
    {
        var a = new Player();
        var b = new Player();
        a.Should().BeEquivalentTo(b);
    }
}
