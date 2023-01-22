using SlipeServer.Server.Elements;
using SlipeServer.Server.Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.ElementCollections;

public class CompoundElementCollection : IElementCollection
{
    public int Count => this.flatElementCollection.Count;

    private readonly FlatElementCollection flatElementCollection;
    private readonly ElementByIdCollection elementByIdCollection;
    private readonly ElementByTypeCollection elementByTypeCollection;
    private readonly ConcurrentDictionary<ElementType, KdTreeElementCollection> spatialCollection;

    public CompoundElementCollection()
    {
        this.flatElementCollection = new();
        this.elementByIdCollection = new();
        this.elementByTypeCollection = new();
        this.spatialCollection = new();
    }

    public void Add(Element element)
    {
        this.flatElementCollection.Add(element);
        this.elementByIdCollection.Add(element);
        this.elementByTypeCollection.Add(element);
        this.GetKdTreeElementCollection(element.ElementType).Add(element);
    }

    public void Remove(Element element)
    {
        this.flatElementCollection.Remove(element);
        this.elementByIdCollection.Remove(element);
        this.elementByTypeCollection.Remove(element);
        this.GetKdTreeElementCollection(element.ElementType).Remove(element);
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
        return this.spatialCollection
            .SelectMany(kvPair => kvPair.Value.GetWithinRange(position, range));
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        return this.GetKdTreeElementCollection(elementType).GetWithinRange<TElement>(position, range, elementType);
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range) where TElement : Element
    {
        return GetWithinRange<TElement>(position, range, ElementTypeHelpers.GetElementType<TElement>());
    }

    private KdTreeElementCollection GetKdTreeElementCollection(ElementType elementType)
    {
        if (!this.spatialCollection.ContainsKey(elementType))
        {
            this.spatialCollection[elementType] = new KdTreeElementCollection();
        }
        return this.spatialCollection[elementType];
    }
}
