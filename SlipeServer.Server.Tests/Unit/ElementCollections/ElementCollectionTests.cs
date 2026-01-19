using FluentAssertions;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.ElementCollections.Concurrent;
using SlipeServer.Server.Elements;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.ElementCollections;

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
    [InlineData(typeof(SpatialHashElementCollection))]
    [InlineData(typeof(SpatialHashCompoundElementCollection))]
    [InlineData(typeof(ConcurrentFlatElementCollection))]
    [InlineData(typeof(ConcurrentElementByTypeCollection))]
    [InlineData(typeof(ConcurrentElementByIdCollection))]
    [InlineData(typeof(SpatialHashCompoundConcurrentElementCollection))]
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
    [InlineData(typeof(SpatialHashElementCollection))]
    [InlineData(typeof(SpatialHashCompoundElementCollection))]
    [InlineData(typeof(ConcurrentFlatElementCollection))]
    [InlineData(typeof(ConcurrentElementByTypeCollection))]
    [InlineData(typeof(ConcurrentElementByIdCollection))]
    [InlineData(typeof(SpatialHashCompoundConcurrentElementCollection))]
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
    [InlineData(typeof(SpatialHashElementCollection))]
    [InlineData(typeof(SpatialHashCompoundElementCollection))]
    [InlineData(typeof(ConcurrentFlatElementCollection))]
    [InlineData(typeof(ConcurrentElementByTypeCollection))]
    [InlineData(typeof(ConcurrentElementByIdCollection))]
    [InlineData(typeof(SpatialHashCompoundConcurrentElementCollection))]
    public void CollectionWithTwoItems_Count_ReturnsTwo(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        collection.Add(new Element() { Id = (ElementId)1 });
        collection.Add(new Element() { Id = (ElementId)2 });

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
    [InlineData(typeof(SpatialHashElementCollection))]
    [InlineData(typeof(SpatialHashCompoundElementCollection))]
    [InlineData(typeof(ConcurrentFlatElementCollection))]
    [InlineData(typeof(ConcurrentElementByTypeCollection))]
    [InlineData(typeof(ConcurrentElementByIdCollection))]
    [InlineData(typeof(SpatialHashCompoundConcurrentElementCollection))]
    public void CollectionWithOneItem_GetById_ReturnsElement(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var element = new Element()
        {
            Id = (ElementId)7
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
    [InlineData(typeof(SpatialHashElementCollection))]
    [InlineData(typeof(SpatialHashCompoundElementCollection))]
    [InlineData(typeof(ConcurrentFlatElementCollection))]
    [InlineData(typeof(ConcurrentElementByTypeCollection))]
    [InlineData(typeof(ConcurrentElementByIdCollection))]
    [InlineData(typeof(SpatialHashCompoundConcurrentElementCollection))]
    public void CollectionWithWithSeveralItems_GetByType_ReturnsCorrectElements(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var objects = new WorldObject[]
        {
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId) 1 },
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId) 2 }
        };

        collection.Add(new Element() { Id = (ElementId)3 });
        collection.Add(new Marker(new(0, 0, 3), MarkerType.Arrow) { Id = (ElementId)4 });
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
    [InlineData(typeof(SpatialHashElementCollection))]
    [InlineData(typeof(SpatialHashCompoundElementCollection))]
    [InlineData(typeof(ConcurrentFlatElementCollection))]
    [InlineData(typeof(ConcurrentElementByTypeCollection))]
    [InlineData(typeof(ConcurrentElementByIdCollection))]
    [InlineData(typeof(SpatialHashCompoundConcurrentElementCollection))]
    public void CollectionWithWithSeveralItems_GetAll_ReturnsAllElements(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var elements = new Element[]
        {
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)1 },
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)2 },
            new Element() { Id = (ElementId)3 },
            new Marker(new(0, 0, 3), MarkerType.Arrow) { Id = (ElementId)4 }
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
    [InlineData(typeof(SpatialHashElementCollection))]
    [InlineData(typeof(SpatialHashCompoundElementCollection))]
    [InlineData(typeof(ConcurrentFlatElementCollection))]
    [InlineData(typeof(ConcurrentElementByTypeCollection))]
    [InlineData(typeof(ConcurrentElementByIdCollection))]
    [InlineData(typeof(SpatialHashCompoundConcurrentElementCollection))]
    public void CollectionWithWithSeveralItems_GetWithinRange_ReturnsAllElementsWithinRange(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var elements = new Element[]
        {
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)1 },
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)2 },
            new Element() { Id = (ElementId)3, Position = new(5, 0, 3) },
            new Marker(new(5, 0, 3), MarkerType.Arrow) { Id = (ElementId)4 }
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
    [InlineData(typeof(SpatialHashElementCollection))]
    [InlineData(typeof(SpatialHashCompoundElementCollection))]
    [InlineData(typeof(ConcurrentFlatElementCollection))]
    [InlineData(typeof(ConcurrentElementByTypeCollection))]
    [InlineData(typeof(ConcurrentElementByIdCollection))]
    [InlineData(typeof(SpatialHashCompoundConcurrentElementCollection))]
    public void CollectionWithWithSeveralItems_GetWithinRangeByType_ReturnsAllElementsWithinRangeByType(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var elements = new Element[]
        {
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)1 },
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)2 },
            new WorldObject(321, new(5, 0, 3)) { Id = (ElementId)3 },
            new Element() { Id = (ElementId)4, Position = new(0, 0, 3) },
            new Marker(new(5, 0, 3), MarkerType.Arrow) { Id = (ElementId)5 }
        };

        foreach (var element in elements)
            collection.Add(element);

        var result = collection.GetWithinRange<WorldObject>(new(0, 0, 3), 1);

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
    [InlineData(typeof(SpatialHashElementCollection))]
    [InlineData(typeof(SpatialHashCompoundElementCollection))]
    [InlineData(typeof(ConcurrentFlatElementCollection))]
    [InlineData(typeof(ConcurrentElementByTypeCollection))]
    [InlineData(typeof(ConcurrentElementByIdCollection))]
    [InlineData(typeof(SpatialHashCompoundConcurrentElementCollection))]
    public void CollectionWithItemsRemoved_ReturnsEmpty(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var elements = new Element[]
        {
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)1 },
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)2 },
            new WorldObject(321, new(5, 0, 3)) { Id = (ElementId)3 },
            new Element() { Id = (ElementId)4, Position = new(0, 0, 3) },
            new Marker(new(5, 0, 3), MarkerType.Arrow) { Id = (ElementId)5 }
        };

        foreach (var element in elements)
            collection.Add(element);

        foreach (var element in elements)
            collection.Remove(element);

        var result = collection.GetAll();

        collection.Count.Should().Be(0);
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(typeof(SpatialHashElementCollection))]
    [InlineData(typeof(RTreeElementCollection))]
    [InlineData(typeof(KdTreeElementCollection))]
    public async Task Collection_WhenItemsAreMovingAndBeingRemovedSimultaneously_ReturnsEmpty(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var elements = new Element[]
        {
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)1 },
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)2 },
            new WorldObject(321, new(5, 0, 3)) { Id = (ElementId)3 },
            new Element() { Id = (ElementId)4, Position = new(0, 0, 3) },
            new Marker(new(5, 0, 3), MarkerType.Arrow) { Id = (ElementId)5 }
        };

        foreach (var element in elements)
            collection.Add(element);

        var tasks = elements.Select(x => Task.Run(() =>
        {
            // move the element so it forces a re-insert
            x.Position += new Vector3(100, 100, 100);
        })).ToArray();

        await Task.Delay(500);

        foreach (var element in elements)
            collection.Remove(element);

        await Task.WhenAll(tasks);
        await Task.Delay(3500);

        var result = collection.GetAll();

        collection.Count.Should().Be(0);
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(typeof(SpatialHashElementCollection))]
    [InlineData(typeof(SpatialHashCompoundElementCollection))]
    [InlineData(typeof(SpatialHashCompoundConcurrentElementCollection))]
    public async Task Collection_WhenElementsWereMoved_ReturnsElementsAtNewPosition(Type type)
    {
        var collection = (IElementCollection)Activator.CreateInstance(type)!;

        var elements = new Element[]
        {
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)1 },
            new WorldObject(321, new(0, 0, 3)) { Id = (ElementId)2 },
            new WorldObject(321, new(5, 0, 3)) { Id = (ElementId)3 },
            new Element() { Id = (ElementId)4, Position = new(0, 0, 3) },
            new Marker(new(5, 0, 3), MarkerType.Arrow) { Id = (ElementId)5 }
        };

        foreach (var element in elements)
            collection.Add(element);

        foreach (var element in elements)
            element.Position += new Vector3(100, 100, 100);

        var result = collection.GetWithinRange(new(100, 100, 100), 10);

        collection.Count.Should().Be(elements.Count());
        result.Should().BeEquivalentTo(elements);
    }

}
