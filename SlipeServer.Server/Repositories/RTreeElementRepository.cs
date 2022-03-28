using System;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SlipeServer.Server.Elements.Events;
using RBush;
using System.Collections.Concurrent;

namespace SlipeServer.Server.Repositories;

internal class RTreeRef : ISpatialData
{
    private readonly Envelope envelope;
    public Element Element { get; }

    public RTreeRef(Element element, Vector3 position)
    {
        this.Element = element;
        this.envelope = new Envelope(
            minX: position.X,
            minY: position.Y,
            maxX: position.X,
            maxY: position.Y);
    }

    public RTreeRef(Element element) : this(element, element.Position)
    {

    }

    public ref readonly Envelope Envelope => ref this.envelope;
}

public class RTreeElementRepository : IElementRepository
{
    public int Count => this.elements.Count;
    private readonly RBush<RTreeRef> elements;
    private readonly ConcurrentDictionary<Element, RTreeRef> elementRefs;
    private object generalLock = new object();

    public RTreeElementRepository()
    {
        this.elements = new();
        this.elementRefs = new();
    }

    public void Add(Element element)
    {
        lock (this.generalLock)
        {
            element.PositionChanged += ReInsertElement;
            var elementRef = new RTreeRef(element);
            this.elements.Insert(elementRef);
            this.elementRefs[element] = elementRef;
        }
    }

    public Element? Get(uint id)
    {
        lock (this.generalLock)
        {
            return this.elements
                .Search()
                .Select(x => x.Element)
                .FirstOrDefault(element => element.Id == id);
        }
    }

    public void Remove(Element element)
    {
        lock (this.generalLock)
        {
            element.PositionChanged -= ReInsertElement;
            if (this.elementRefs.Remove(element, out var value))
                this.elements.Delete(value);
        }
    }

    public IEnumerable<Element> GetAll()
    {
        lock (this.generalLock)
        {
            return this.elements
            .Search()
            .Select(x => x.Element);
        }
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        lock (this.generalLock)
        {
            return this.elements
            .Search()
            .Select(x => x.Element)
            .Where(element => element.ElementType == elementType)
            .Cast<TElement>();
        }
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        lock (this.generalLock)
        {
            return this.elements
                .Search(new Envelope(position.X - range, position.Y - range, position.X + range, position.Y + range))
                .Select(x => x.Element);
        }
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        lock (this.generalLock)
        {
            return this.GetWithinRange(position, range)
                .Where(element => element.ElementType == elementType)
                .Cast<TElement>();
        }
    }

    private void ReInsertElement(Element element, ElementChangedEventArgs<Vector3> args)
    {
        lock (this.generalLock)
        {
            this.elements.Delete(this.elementRefs[element]);

            var elementRef = new RTreeRef(element);
            this.elements.Insert(elementRef);
            this.elementRefs[element] = elementRef;
        }
    }
}
