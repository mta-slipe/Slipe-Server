using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.ElementCollections.Concurrent;

public class ConcurrentElementByIdCollection : IElementCollection
{
    public int Count => this.elements.Count;

    private readonly ConcurrentDictionary<ElementId, Element> elements = new();

    public void Add(Element element)
    {
        this.elements.TryAdd(element.Id, element);
    }

    public Element? Get(uint id)
    {
        if (this.elements.TryGetValue((ElementId)id, out var element))
            return element;

        return null;
    }

    public void Remove(Element element)
    {
        this.elements.TryRemove(element.Id, out var _);
    }

    public IEnumerable<Element> GetAll()
    {
        var value = this.elements.Values.ToArray();
        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        var value = this.elements.Values
            .Where(element => element.ElementType == elementType)
            .Cast<TElement>()
            .ToArray();

        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>() where TElement : Element
    {
        return this.GetByType<TElement>(ElementTypeHelpers.GetElementType<TElement>());
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        var rangeSquared = range * range;

        var value = this.elements
            .Where(kvPair => Vector3.DistanceSquared(kvPair.Value.Position, position) < rangeSquared)
            .Select(kvPair => kvPair.Value)
            .ToArray();

        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        var rangeSquared = range * range;

        var value = this.elements
            .Where(element => Vector3.DistanceSquared(element.Value.Position, position) < rangeSquared && element.Value.ElementType == elementType)
            .Select(kvPair => kvPair.Value)
            .Cast<TElement>()
            .ToArray();

        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range) where TElement : Element
    {
        return GetWithinRange<TElement>(position, range, ElementTypeHelpers.GetElementType<TElement>());
    }
}
