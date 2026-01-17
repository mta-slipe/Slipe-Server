using SlipeServer.Server.Elements;
using SlipeServer.Server.Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.ElementCollections.Concurrent;

public class ConcurrentFlatElementCollection : IElementCollection
{
    public int Count => this.elements.Count;

    private readonly ConcurrentDictionary<Element, byte> elements = [];

    public void Add(Element element)
    {
        this.elements.TryAdd(element, 0);
    }

    public Element? Get(uint id)
    {
        var value = this.elements.Keys.FirstOrDefault(element => element.Id.Value == id);

        return value;
    }

    public void Remove(Element element)
    {
        this.elements.TryRemove(element, out var _);
    }

    public IEnumerable<Element> GetAll()
    {
        var value = this.elements
            .Keys
            .ToArray();

        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        var value = this.elements.Keys
            .Where(element => element.ElementType == elementType)
            .Cast<TElement>()
            .ToArray();

        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>() where TElement : Element
    {
        var value = this.GetByType<TElement>(ElementTypeHelpers.GetElementType<TElement>());
        return value;
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        var value = this.elements.Keys
            .Where(element => Vector3.Distance(element.Position, position) < range)
            .ToArray();

        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        var value = this.elements.Keys
            .Where(element => Vector3.Distance(element.Position, position) < range && element.ElementType == elementType)
            .Cast<TElement>()
            .ToArray();

        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range) where TElement : Element
    {
        return GetWithinRange<TElement>(position, range, ElementTypeHelpers.GetElementType<TElement>());
    }
}
