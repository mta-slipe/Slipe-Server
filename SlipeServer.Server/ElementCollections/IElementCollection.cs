using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.ElementCollections;

/// <summary>
/// Represents an arbitrary collection of elements, used (among others) as a repository of all elements associated with the server.
/// Specific implementations of this can have optimised implementations for certain methods, like getting elements within range of a certain position.
/// </summary>
public interface IElementCollection
{
    int Count { get; }

    void Add(Element element);
    void Remove(Element element);

    Element? Get(uint id);
    IEnumerable<Element> GetAll();
    IEnumerable<Element> GetWithinRange(Vector3 position, float range);
    IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element;
    IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range) where TElement : Element;
    IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element;
    IEnumerable<TElement> GetByType<TElement>() where TElement : Element;
}
