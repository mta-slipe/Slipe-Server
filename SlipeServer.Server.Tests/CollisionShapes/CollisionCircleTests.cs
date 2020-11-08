﻿using FluentAssertions;
using SlipeServer.Server.Elements.ColShapes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;

namespace SlipeServer.Server.Tests.CollisionShapes
{
    public class CollisionCircleTests
    {
        [Theory]
        [InlineData(1, 0, 0, 3)]
        [InlineData(1, 1, 0, 3)]
        [InlineData(1, 1, 1, 3)]
        [InlineData(1, 1, 50, 3)]
        public void PointWithinReturnsTrueTest(float x, float y, float z, float radius)
        {
            var shape = new CollisionCircle(Vector2.Zero, radius);

            var result = shape.IsWithin(new Vector3(x, y, z));

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(1, 0, 0, 0.5f)]
        [InlineData(0, 1, 0, 0.5f)]
        public void PointOutsideReturnsFalseTest(float x, float y, float z, float radius)
        {
            var shape = new CollisionCircle(Vector2.Zero, radius);

            var result = shape.IsWithin(new Vector3(x, y, z));

            result.Should().BeFalse();
        }
    }
}