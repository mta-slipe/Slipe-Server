using FluentAssertions;
using SlipeServer.Server.Elements;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Markers;

public class MarkerCollisionTests
{
    [Theory]
    [InlineData(1, 0, 1, 6)]
    [InlineData(1, 1, 1, 6)]
    [InlineData(1, 1, 5, 6)]
    [InlineData(.5f, .5f, .75f, 2)]
    public void CylinderMarkerPointWithinTests(float x, float y, float z, float size)
    {
        var marker = new Marker(Vector3.Zero, MarkerType.Cylinder, true)
        {
            Size = size
        };
        var shape = marker.ColShape!;

        var result = shape.IsWithin(new Vector3(x, y, z));

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(4, 0, 1, 6)]
    [InlineData(1, 4, 1, 6)]
    [InlineData(1, 1, 8, 6)]
    [InlineData(.5f, .5f, 2.5f, 2)]
    public void CylinderMarkerPointOutsideTests(float x, float y, float z, float size)
    {
        var marker = new Marker(Vector3.Zero, MarkerType.Cylinder, true)
        {
            Size = size
        };
        var shape = marker.ColShape!;

        var result = shape.IsWithin(new Vector3(x, y, z));

        result.Should().BeFalse();
    }
}
