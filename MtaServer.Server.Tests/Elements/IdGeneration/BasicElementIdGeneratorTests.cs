using FluentAssertions;
using MtaServer.Server.Constants;
using MtaServer.Server.Elements.IdGeneration;
using Xunit;

namespace MtaServer.Server.Tests.Elements.IdGeneration
{
    public class BasicElementIdGeneratorTests
    {
        [Fact]
        public void GetId_ShouldReturnUniqueIds()
        {
            var generator = new BasicElementIdGenerator();

            var first = generator.GetId();
            var second = generator.GetId();

            first.Should().NotBe(second);
        }

        [Fact]
        public void GetId_ShouldWrapAround()
        {
            var generator = new BasicElementIdGenerator();

            for (int i = 0; i < ElementConstants.MaxElementId - 1; i++)
            {
                generator.GetId();
            }

            var id = generator.GetId();
            id.Should().Be(0);
        }
    }
}
