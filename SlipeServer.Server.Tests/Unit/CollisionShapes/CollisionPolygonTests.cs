using FluentAssertions;
using SlipeServer.Server.Elements.ColShapes;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.CollisionShapes;

public class CollisionPolygonTests
{
    private readonly CollisionPolygon shape = new CollisionPolygon(Vector3.Zero, [
        new(1, 2),
        new(1, 4),
        new(2, 3),
        new(3, 5),
        new(4, 3),
        new(5, 4),
        new(6, 2),
        new(4, 1),
    ])
    {
        Height = new(-5, 5)
    };


    [Theory]
    [InlineData(1.25f, 3.5f)]
    [InlineData(3f, 3.5f)]
    [InlineData(5f, 3.5f)]
    public void PointWithinReturnsTrueTest(float x, float y)
    {
        var result = this.shape.IsWithin(new Vector3(x, y, 0));

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.75f, 3.5f)]
    [InlineData(2, 3.5f)]
    [InlineData(4, 3.5f)]
    public void PointOutsideReturnsFalseTest(float x, float y)
    {
        var result = this.shape.IsWithin(new Vector3(x, y, 0));

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1.25f, 3.5f, 7.5f)]
    [InlineData(3f, 3.5f, -7.5f)]
    [InlineData(5f, 3.5f, 5.001f)]
    public void ThreeDimensionalPointOutsideReturnsFalseTest(float x, float y, float z)
    {
        var result = this.shape.IsWithin(new Vector3(x, y, z));

        result.Should().BeFalse();
    }



    private readonly CollisionPolygon shapeTwo = new CollisionPolygon(Vector3.Zero, [
        new(25.825f, 24.756f),
        new(25.825f, 43.944f),
        new(0.422f, 43.944f),
        new(0.422f, 18.66f),
        new(20.293f, 18.66f),
    ])
    {
        Height = new(-5, 5)
    };


    [Theory]
    [InlineData(8f, 30f)]
    public void PointWithinShapeTwoeturnsTrueTest(float x, float y)
    {
        var result = this.shapeTwo.IsWithin(new Vector3(x, y, 0));

        result.Should().BeTrue();
    }

}
