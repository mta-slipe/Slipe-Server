using FluentAssertions;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System;
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

}
