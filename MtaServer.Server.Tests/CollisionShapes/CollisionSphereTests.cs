using FluentAssertions;
using MtaServer.Server.Elements.ColShapes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;

namespace MtaServer.Server.Tests.CollisionShapes
{
    public class CollisionSphereTests
    {
        [Theory]
        [InlineData(1, 0, 0, 3)]
        [InlineData(1, 1, 0, 3)]
        [InlineData(1, 1, 1, 3)]
        public void PointWithinReturnsTrueTest(float x, float y, float z, float radius)
        {
            var shape = new CollisionSphere(Vector3.Zero, radius);

            var result = shape.IsWithin(new Vector3(x, y, z));

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(1, 0, 0, 0.5f)]
        [InlineData(0, 1, 0, 0.5f)]
        [InlineData(0, 0, 1, 0.5f)]
        public void PointOutsideReturnsFalseTest(float x, float y, float z, float radius)
        {
            var shape = new CollisionSphere(Vector3.Zero, radius);

            var result = shape.IsWithin(new Vector3(x, y, z));

            result.Should().BeFalse();
        }
    }
}
