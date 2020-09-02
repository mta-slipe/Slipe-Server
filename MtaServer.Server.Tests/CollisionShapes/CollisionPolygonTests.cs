using FluentAssertions;
using MtaServer.Server.Elements.ColShapes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;

namespace MtaServer.Server.Tests.CollisionShapes
{
    public class CollisionPolygonTests
    {
        private readonly CollisionPolygon shape = new CollisionPolygon(Vector3.Zero, new Vector2[] {
            new Vector2(1, 2),
            new Vector2(1, 4),
            new Vector2(2, 3),
            new Vector2(3, 5),
            new Vector2(4, 3),
            new Vector2(5, 4),
            new Vector2(6, 2),
            new Vector2(4, 1),
        });


        [Theory]
        [InlineData(1.25f, 3.5f)]
        [InlineData(3f, 3.5f)]
        [InlineData(5f, 3.5f)]
        public void PointWithinReturnsTrueTest(float x, float y)
        {
            var result = shape.IsWithin(new Vector3(x, y, 0));

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(0.75f, 3.5f)]
        [InlineData(2, 3.5f)]
        [InlineData(4, 3.5f)]
        public void PointOutsideReturnsFalseTest(float x, float y)
        {
            var result = shape.IsWithin(new Vector3(x, y, 0));

            result.Should().BeFalse();
        }
    }
}
