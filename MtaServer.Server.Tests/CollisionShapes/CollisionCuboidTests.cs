using FluentAssertions;
using MtaServer.Server.Elements.ColShapes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;

namespace MtaServer.Server.Tests.CollisionShapes
{
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
    }
}
