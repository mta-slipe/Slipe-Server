using FluentAssertions;
using SlipeServer.Server.Elements.ColShapes;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.CollisionShapes;

public class CollisionTubeTests
{
    [Theory]
    [InlineData(1, 0, 1, 3)]
    [InlineData(1, 1, 1, 3)]
    [InlineData(1, 1, 8, 3)]
    public void PointWithinReturnsTrueTest(float x, float y, float z, float radius)
    {
        var shape = new CollisionTube(Vector3.Zero, radius, 10);

        var result = shape.IsWithin(new Vector3(x, y, z));

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, 0, 0, 0.5f)]
    [InlineData(0, 1, 0, 0.5f)]
    [InlineData(1, 1, 50, 3)]
    public void PointOutsideReturnsFalseTest(float x, float y, float z, float radius)
    {
        var shape = new CollisionTube(Vector3.Zero, radius, 10);

        var result = shape.IsWithin(new Vector3(x, y, z));

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 0, 101, 3)]
    [InlineData(1, 1, 101, 3)]
    [InlineData(1, 1, 108, 3)]
    public void PointWithinAtDifferentPositionReturnsTrueTest(float x, float y, float z, float radius)
    {
        var shape = new CollisionTube(new Vector3(0, 0, 100), radius, 10);

        var result = shape.IsWithin(new Vector3(x, y, z));

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, 0, 100, 0.5f)]
    [InlineData(0, 1, 100, 0.5f)]
    [InlineData(1, 1, 150, 3)]
    public void PointOutsideAtDifferentPositionReturnsFalseTest(float x, float y, float z, float radius)
    {
        var shape = new CollisionTube(new Vector3(0, 0, 100), radius, 10);

        var result = shape.IsWithin(new Vector3(x, y, z));

        result.Should().BeFalse();
    }
}
