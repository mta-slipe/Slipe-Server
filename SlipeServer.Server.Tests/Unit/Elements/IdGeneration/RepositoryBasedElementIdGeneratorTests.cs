using FluentAssertions;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.IdGeneration;
using SlipeServer.Server.Repositories;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Elements.IdGeneration;

public class RepositoryBasedElementIdGeneratorTests
{
    [Fact]
    public void GetId_ShouldReturnUniqueIds()
    {
        var repository = new ElementByIdRepository();
        var generator = new RepositoryBasedElementIdGenerator(repository);

        var first = generator.GetId();
        repository.Add(new DummyElement() { Id = first });

        var second = generator.GetId();
        repository.Add(new DummyElement() { Id = second });

        first.Should().NotBe(second);
    }

    [Fact]
    public void GetId_ShouldReturnUnusedId()
    {
        var repository = new ElementByIdRepository();
        var dummyElement = new DummyElement()
        {
            Id = 0
        };
        repository.Add(dummyElement);
        var generator = new RepositoryBasedElementIdGenerator(repository);

        var id = generator.GetId();

        id.Should().NotBe(dummyElement.Id);
    }

    [Fact]
    public void GetId_ShouldWrapAround()
    {
        var repository = new ElementByIdRepository();
        var generator = new RepositoryBasedElementIdGenerator(repository);

        var first = generator.GetId();
        var firstElement = new DummyElement() { Id = first };
        repository.Add(firstElement);

        for (int i = 0; i < ElementConstants.MaxElementId - 2; i++)
        {
            var id = generator.GetId();
            repository.Add(new DummyElement() { Id = id });
        }

        repository.Remove(firstElement);

        var finalId = generator.GetId();
        finalId.Should().Be(first);
    }

    [Fact]
    public void GetId_ThrowsExceptionWhenOutOfElementIds()
    {
        var repository = new ElementByIdRepository();
        var generator = new RepositoryBasedElementIdGenerator(repository);

        for (int i = 0; i < ElementConstants.MaxElementId - 1; i++)
        {
            var second = generator.GetId();
            repository.Add(new DummyElement() { Id = second });
        }

        bool exceptionThrown = false;
        try
        {
            generator.GetId();
        }
        catch (ElementIdsExhaustedException)
        {
            exceptionThrown = true;
        }

        exceptionThrown.Should().BeTrue();
    }
}
