using FluentAssertions;
using MtaServer.Server.Elements.IdGeneration;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MtaServer.Server.Tests
{
    public class IdGeneratorTests
    {
        [Fact]
        public void GetId_ShouldReturnUniqueIds()
        {
            var generator = new ElementIdGenerator();

            var first = generator.GetId();
            var second = generator.GetId();

            first.Should().NotBe(second);
        }
    }
}
