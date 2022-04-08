using System;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.Repositories;

public class ElementByTypeRepository : IElementRepository
{
    public int Count => this.elements.Count;

    private readonly Dictionary<ElementType, List<Element>> elements;

    public ElementByTypeRepository()
    {
        this.elements = new Dictionary<ElementType, List<Element>>();
    }

    public void Add(Element element)
    {
        if (!this.elements.ContainsKey(element.ElementType))
        {
            this.elements[element.ElementType] = new List<Element>();
        }
        this.elements[element.ElementType].Add(element);
    }

    public Element? Get(uint id)
    {
        foreach (var list in this.elements.Values)
        {
            var element = list.FirstOrDefault(element => element.Id == id);
            if (element != null)
            {
                return element;
            }
        }
        return null;
    }

    public void Remove(Element element)
    {
        if (this.elements.ContainsKey(element.ElementType))
            this.elements[element.ElementType].Remove(element);
    }

    public IEnumerable<Element> GetAll()
    {
        return this.elements.SelectMany(kvPair => kvPair.Value);
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        return this.elements.ContainsKey(elementType) ? this.elements[elementType].ToArray().Cast<TElement>() : Array.Empty<TElement>();
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        return this.elements
            .SelectMany(kvPair => kvPair.Value)
            .Where(element => Vector3.Distance(element.Position, position) < range);
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        return this.elements.ContainsKey(elementType) ?
            this.elements[elementType].Cast<TElement>()
                .Where(element => Vector3.Distance(element.Position, position) < range) :
            Array.Empty<TElement>();
    }
}
