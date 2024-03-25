using FluentAssertions;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.IdGeneration;
using SlipeServer.Server.ElementCollections;
using Xunit;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Server.Tests.Unit.Elements.IdGeneration;

public class CollectionBasedElementIdGeneratorTests
{
    [Fact]
    public void GetId_ShouldReturnUniqueIds()
    {
        var collection = new ElementByIdCollection();
        var generator = new CollectionBasedElementIdGenerator(collection);

        var first = generator.GetId();
        collection.Add(new DummyElement() { Id = (ElementId)first });

        var second = generator.GetId();
        collection.Add(new DummyElement() { Id = (ElementId)second });

        first.Should().NotBe(second);
    }

    [Fact]
    public void GetId_ShouldReturnUnusedId()
    {
        var collection = new ElementByIdCollection();
        var dummyElement = new DummyElement()
        {
            Id = (ElementId)1
        };
        collection.Add(dummyElement);
        var generator = new CollectionBasedElementIdGenerator(collection);

        var id = generator.GetId();

        id.Should().NotBe(dummyElement.Id.Value);
    }

    [Fact]
    public void GetId_ShouldReturnOneAsFirstId()
    {
        var collection = new ElementByIdCollection();
        var generator = new CollectionBasedElementIdGenerator(collection);

        var id = generator.GetId();

        id.Should().Be(1);
    }

    [Fact]
    public void GetId_ShouldWrapAround()
    {
        var collection = new ElementByIdCollection();
        var generator = new CollectionBasedElementIdGenerator(collection);

        var first = generator.GetId();
        var firstElement = new DummyElement() { Id = (ElementId)first };
        collection.Add(firstElement);

        for (int i = 0; i < ElementConstants.MaxElementId - 1; i++)
        {
            var id = generator.GetId();
            collection.Add(new DummyElement() { Id = (ElementId)id });
        }

        collection.Remove(firstElement);

        var finalId = generator.GetId();
        finalId.Should().Be(first);
    }

    [Fact]
    public void GetId_ThrowsExceptionWhenOutOfElementIds()
    {
        var collection = new ElementByIdCollection();
        var generator = new CollectionBasedElementIdGenerator(collection);

        for (int i = 0; i < ElementConstants.MaxElementId; i++)
        {
            var second = generator.GetId();
            collection.Add(new DummyElement() { Id = (ElementId)second });
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
