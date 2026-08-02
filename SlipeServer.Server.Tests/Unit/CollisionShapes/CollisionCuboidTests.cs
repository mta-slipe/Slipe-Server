using FluentAssertions;
using SlipeServer.Server.Elements.ColShapes;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.CollisionShapes;

public class CollisionCuboidTests
{
    [Theory]
    [InlineData(0.5f, 0.5f, 0.5f)]
    [InlineData(0.25f, 0.75f, 0.5f)]
    [InlineData(0.1f, 0.9f, 0.1f)]
    public void PointWithinReturnsTrueTest(float x, float y, float z)
    {
        var shape = new CollisionCuboid(Vector3.Zero, Vector3.One);

        var result = shape.IsWithin(new Vector3(x, y, z));

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.5f, 1.5f, 0.5f)]
    [InlineData(1.25f, 0.75f, 0.5f)]
    [InlineData(0.1f, 0.9f, 1.1f)]
    public void PointOutsideReturnsFalseTest(float x, float y, float z)
    {
        var shape = new CollisionCuboid(Vector3.Zero, Vector3.One);

        var result = shape.IsWithin(new Vector3(x, y, z));

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(10, 2, 4, 5, 1, 1)]
    [InlineData(10, 2, 4, 2, 1.75f, 3)]
    [InlineData(10, 2, 4, 8, 0.25, 0.25)]
    public void PointWithinWithNonUniformSizeReturnsTrueTest(float width, float depth, float height, float x, float y, float z)
    {
        var shape = new CollisionCuboid(Vector3.Zero, new Vector3(width, depth, height));

        var result = shape.IsWithin(new Vector3(x, y, z));

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(10, 2, 4, 12, 1, 1)]
    [InlineData(10, 2, 4, 2, -1.75f, 3)]
    [InlineData(10, 2, 4, 8, 0.25, 4.25)]
    public void PointOutsidenWithNonUniformSizeReturnsFalseTest(float width, float depth, float height, float x, float y, float z)
    {
        var shape = new CollisionCuboid(Vector3.Zero, new Vector3(width, depth, height));

        var result = shape.IsWithin(new Vector3(x, y, z));

        result.Should().BeFalse();
    }
}
