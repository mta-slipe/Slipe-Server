using SlipeServer.Server.Elements;
using SlipeServer.Server.Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.ElementCollections.Concurrent;

public class ConcurrentElementByTypeCollection : IElementCollection
{
    public int Count => this.elements.Select(x => x.Value.Count).Sum();

    private readonly ConcurrentDictionary<ElementType, ConcurrentDictionary<Element, byte>> elements = [];


    public void Add(Element element)
    {
        if (!this.elements.ContainsKey(element.ElementType))
        {
            this.elements[element.ElementType] = [];
        }

        this.elements[element.ElementType].TryAdd(element, 0);
    }

    public Element? Get(uint id)
    {
        foreach (var list in this.elements.Values)
        {
            var element = list.Keys.FirstOrDefault(element => element.Id.Value == id);
            if (element != null)
            {
                return element;
            }
        }

        return null;
    }

    public void Remove(Element element)
    {
        if (this.elements.TryGetValue(element.ElementType, out var value))
        {
            value.TryRemove(element, out var _);
        }
    }

    public IEnumerable<Element> GetAll()
    {
        var value = this.elements
            .SelectMany(kvPair => kvPair.Value.Keys)
            .ToArray();

        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        if (this.elements.TryGetValue(elementType, out var elements))
            return [.. elements.Keys.Cast<TElement>()];

        return [];
    }

    public IEnumerable<TElement> GetByType<TElement>() where TElement : Element
    {
        var value = this.GetByType<TElement>(ElementTypeHelpers.GetElementType<TElement>());
        return value;
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        var rangeSquared = range * range;

        var value = this.elements
            .SelectMany(kvPair => kvPair.Value.Keys)
            .Where(element => Vector3.DistanceSquared(element.Position, position) < rangeSquared)
            .ToArray();

        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        var rangeSquared = range * range;

        return this.GetByType<TElement>(elementType)
            .Where(element => Vector3.DistanceSquared(element.Position, position) < rangeSquared);
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range) where TElement : Element
    {
        return GetWithinRange<TElement>(position, range, ElementTypeHelpers.GetElementType<TElement>());
    }
}
