using SlipeServer.Server.Elements;
using SlipeServer.Server.Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.ElementCollections.Concurrent;

public class SpatialHashCompoundConcurrentElementCollection() : IElementCollection
{
    public int Count => this.flatElementCollection.Count;

    private readonly ConcurrentFlatElementCollection flatElementCollection = new();
    private readonly ConcurrentElementByIdCollection elementByIdCollection = new();
    private readonly ConcurrentElementByTypeCollection elementByTypeCollection = new();
    private readonly ConcurrentDictionary<ElementType, SpatialHashElementCollection> spatialCollections = new();

    public void Add(Element element)
    {
        this.flatElementCollection.Add(element);
        this.elementByIdCollection.Add(element);
        this.elementByTypeCollection.Add(element);
        this.GetSpatialHashCollectionForType(element.ElementType).Add(element);
    }

    public void Remove(Element element)
    {
        this.flatElementCollection.Remove(element);
        this.elementByIdCollection.Remove(element);
        this.elementByTypeCollection.Remove(element);
        this.GetSpatialHashCollectionForType(element.ElementType).Remove(element);
    }

    public Element? Get(uint id)
    {
        return this.elementByIdCollection.Get(id);
    }

    public IEnumerable<Element> GetAll()
    {
        return this.flatElementCollection.GetAll();
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        return this.elementByTypeCollection.GetByType<TElement>(elementType);
    }

    public IEnumerable<TElement> GetByType<TElement>() where TElement : Element
    {
        return this.GetByType<TElement>(ElementTypeHelpers.GetElementType<TElement>());
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        return this.spatialCollections
            .SelectMany(kvPair => kvPair.Value.GetWithinRange(position, range));
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        return this.GetSpatialHashCollectionForType(elementType).GetWithinRange<TElement>(position, range, elementType);
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range) where TElement : Element
    {
        return GetWithinRange<TElement>(position, range, ElementTypeHelpers.GetElementType<TElement>());
    }

    private SpatialHashElementCollection GetSpatialHashCollectionForType(ElementType elementType)
    {
        if (this.spatialCollections.TryGetValue(elementType, out var value))
            return value;

        value = new SpatialHashElementCollection();
        this.spatialCollections[elementType] = value;
        return value;
    }
}
