using FluentAssertions;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System;
using System.Linq;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.ElementCollecions;

public class ElementCollectionTests
{
    [Theory]
    [InlineData(typeof(ElementByIdCollection))]
    [InlineData(typeof(ElementByTypeCollection))]
    [InlineData(typeof(FlatElementCollection))]
    [InlineData(typeof(RTreeElementCollection))]
    [InlineData(typeof(KdTreeElementCollection))]
    [InlineData(typeof(CompoundElementCollection))]
    [InlineData(typeof(BasicCompoundElementCollection))]
    [InlineData(typeof(RTreeCompoundElementCollection))]
    public void NewCollection_Count_ReturnsZero(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        collection.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(typeof(ElementByIdCollection))]
    [InlineData(typeof(ElementByTypeCollection))]
    [InlineData(typeof(FlatElementCollection))]
    [InlineData(typeof(RTreeElementCollection))]
    [InlineData(typeof(KdTreeElementCollection))]
    [InlineData(typeof(CompoundElementCollection))]
    [InlineData(typeof(BasicCompoundElementCollection))]
    [InlineData(typeof(RTreeCompoundElementCollection))]
    public void CollectionWithOneItem_Count_ReturnsOne(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        collection.Add(new Element());

        collection.Count.Should().Be(1);
    }

    [Theory]
    [InlineData(typeof(ElementByIdCollection))]
    [InlineData(typeof(ElementByTypeCollection))]
    [InlineData(typeof(FlatElementCollection))]
    [InlineData(typeof(RTreeElementCollection))]
    [InlineData(typeof(KdTreeElementCollection))]
    [InlineData(typeof(CompoundElementCollection))]
    [InlineData(typeof(BasicCompoundElementCollection))]
    [InlineData(typeof(RTreeCompoundElementCollection))]
    public void CollectionWithTwoItems_Count_ReturnsTwo(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        collection.Add(new Element() { Id = 1 });
        collection.Add(new Element() { Id = 2 });

        collection.Count.Should().Be(2);
    }

    [Theory]
    [InlineData(typeof(ElementByIdCollection))]
    [InlineData(typeof(ElementByTypeCollection))]
    [InlineData(typeof(FlatElementCollection))]
    [InlineData(typeof(RTreeElementCollection))]
    [InlineData(typeof(KdTreeElementCollection))]
    [InlineData(typeof(CompoundElementCollection))]
    [InlineData(typeof(BasicCompoundElementCollection))]
    [InlineData(typeof(RTreeCompoundElementCollection))]
    public void CollectionWithOneItem_GetById_ReturnsElement(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var element = new Element()
        {
            Id = 7
        };

        collection.Add(element);

        var result = collection.Get(element.Id);

        result.Should().Be(element);
    }

    [Theory]
    [InlineData(typeof(ElementByIdCollection))]
    [InlineData(typeof(ElementByTypeCollection))]
    [InlineData(typeof(FlatElementCollection))]
    [InlineData(typeof(RTreeElementCollection))]
    [InlineData(typeof(KdTreeElementCollection))]
    [InlineData(typeof(CompoundElementCollection))]
    [InlineData(typeof(BasicCompoundElementCollection))]
    [InlineData(typeof(RTreeCompoundElementCollection))]
    public void CollectionWithWithSeveralItems_GetByType_ReturnsCorrectElements(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var objects = new WorldObject[]
        {
            new WorldObject(321, new(0, 0, 3)) { Id = 1 },
            new WorldObject(321, new(0, 0, 3)) { Id = 2 }
        };

        collection.Add(new Element() { Id = 3 });
        collection.Add(new Marker(new(0, 0, 3), MarkerType.Arrow) { Id = 4 });
        foreach (var worldObject in objects)
            collection.Add(worldObject);

        var result = collection.GetByType<WorldObject>();

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(objects);
    }


    [Theory]
    [InlineData(typeof(ElementByIdCollection))]
    [InlineData(typeof(ElementByTypeCollection))]
    [InlineData(typeof(FlatElementCollection))]
    [InlineData(typeof(RTreeElementCollection))]
    [InlineData(typeof(KdTreeElementCollection))]
    [InlineData(typeof(CompoundElementCollection))]
    [InlineData(typeof(BasicCompoundElementCollection))]
    [InlineData(typeof(RTreeCompoundElementCollection))]
    public void CollectionWithWithSeveralItems_GetAll_ReturnsAllElements(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var elements = new Element[]
        {
            new WorldObject(321, new(0, 0, 3)) { Id = 1 },
            new WorldObject(321, new(0, 0, 3)) { Id = 2 },
            new Element() { Id = 3 },
            new Marker(new(0, 0, 3), MarkerType.Arrow) { Id = 4 }
        };

        foreach (var element in elements)
            collection.Add(element);

        var result = collection.GetAll();

        result.Should().HaveCount(4);
        result.Should().BeEquivalentTo(elements);
    }

    [Theory]
    [InlineData(typeof(ElementByIdCollection))]
    [InlineData(typeof(ElementByTypeCollection))]
    [InlineData(typeof(FlatElementCollection))]
    [InlineData(typeof(RTreeElementCollection))]
    [InlineData(typeof(KdTreeElementCollection))]
    [InlineData(typeof(CompoundElementCollection))]
    [InlineData(typeof(BasicCompoundElementCollection))]
    [InlineData(typeof(RTreeCompoundElementCollection))]
    public void CollectionWithWithSeveralItems_GetWithinRange_ReturnsAllElementsWithinRange(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var elements = new Element[]
        {
            new WorldObject(321, new(0, 0, 3)) { Id = 1 },
            new WorldObject(321, new(0, 0, 3)) { Id = 2 },
            new Element() { Id = 3, Position = new(5, 0, 3) },
            new Marker(new(5, 0, 3), MarkerType.Arrow) { Id = 4 }
        };

        foreach (var element in elements)
            collection.Add(element);

        var result = collection.GetWithinRange(new(0, 0, 3), 1);

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(elements.Take(2));
    }

    [Theory]
    [InlineData(typeof(ElementByIdCollection))]
    [InlineData(typeof(ElementByTypeCollection))]
    [InlineData(typeof(FlatElementCollection))]
    [InlineData(typeof(RTreeElementCollection))]
    [InlineData(typeof(KdTreeElementCollection))]
    [InlineData(typeof(CompoundElementCollection))]
    [InlineData(typeof(BasicCompoundElementCollection))]
    [InlineData(typeof(RTreeCompoundElementCollection))]
    public void CollectionWithWithSeveralItems_GetWithinRangeByType_ReturnsAllElementsWithinRangeByType(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var elements = new Element[]
        {
            new WorldObject(321, new(0, 0, 3)) { Id = 1 },
            new WorldObject(321, new(0, 0, 3)) { Id = 2 },
            new WorldObject(321, new(5, 0, 3)) { Id = 3 },
            new Element() { Id = 4, Position = new(0, 0, 3) },
            new Marker(new(5, 0, 3), MarkerType.Arrow) { Id = 5 }
        };

        foreach (var element in elements)
            collection.Add(element);

        var result = collection.GetWithinRange<WorldObject>(new(0, 0, 3), 1);

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(elements.Take(2));
    }

}
