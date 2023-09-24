using RBush;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace SlipeServer.Server.ElementCollections;

internal class RTreeRef : ISpatialData
{
    private readonly Envelope envelope;
    public Element Element { get; }

    public RTreeRef(Element element, Vector3 position)
    {
        this.Element = element;
        this.envelope = new Envelope(
            MinX: position.X,
            MinY: position.Y,
            MaxX: position.X,
            MaxY: position.Y);
    }

    public RTreeRef(Element element) : this(element, element.Position)
    {

    }

    public ref readonly Envelope Envelope => ref this.envelope;
}

public class RTreeElementCollection : IElementCollection
{
    public int Count => this.elements.Count;
    private readonly RBush<RTreeRef> elements;
    private readonly ConcurrentDictionary<Element, RTreeRef> elementRefs;
    private readonly ReaderWriterLockSlim slimLock = new();

    public RTreeElementCollection()
    {
        this.elements = new();
        this.elementRefs = new();
    }

    public void Add(Element element)
    {
        this.slimLock.EnterWriteLock();

        element.PositionChanged += ReInsertElement;
        var elementRef = new RTreeRef(element);
        this.elements.Insert(elementRef);
        this.elementRefs[element] = elementRef;

        this.slimLock.ExitWriteLock();
    }

    public Element? Get(uint id)
    {
        this.slimLock.EnterReadLock();

        var results = this.elements
            .Search()
            .Select(x => x.Element)
            .FirstOrDefault(element => element.Id.Value == id);

        this.slimLock.ExitReadLock();

        return results;
    }

    public void Remove(Element element)
    {
        this.slimLock.EnterWriteLock();

        element.PositionChanged -= ReInsertElement;
        if (this.elementRefs.Remove(element, out var value))
            this.elements.Delete(value);

        this.slimLock.ExitWriteLock();
    }

    public IEnumerable<Element> GetAll()
    {
        this.slimLock.EnterReadLock();

        var results = this.elements
            .Search()
            .Select(x => x.Element);

        this.slimLock.ExitReadLock();

        return results;
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        this.slimLock.EnterReadLock();

        var results = this.elements
            .Search()
            .Select(x => x.Element)
            .Where(element => element.ElementType == elementType)
            .Cast<TElement>();

        this.slimLock.ExitReadLock();

        return results;
    }

    public IEnumerable<TElement> GetByType<TElement>() where TElement : Element
    {
        return this.GetByType<TElement>(ElementTypeHelpers.GetElementType<TElement>());
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        this.slimLock.EnterReadLock();

        var results = this.elements
            .Search(new Envelope(position.X - range, position.Y - range, position.X + range, position.Y + range))
            .Select(x => x.Element)
            .ToArray();

        this.slimLock.ExitReadLock();

        return results;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        var results = this.GetWithinRange(position, range)
            .Where(element => element.ElementType == elementType)
            .Cast<TElement>();

        return results;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range) where TElement : Element
    {
        return GetWithinRange<TElement>(position, range, ElementTypeHelpers.GetElementType<TElement>());
    }

    private void ReInsertElement(Element element, ElementChangedEventArgs<Vector3> args)
    {
        try
        {
            var reference = this.elementRefs[element];
            try
            {
                this.slimLock.EnterWriteLock();
                this.elements.Delete(reference);

                var elementRef = new RTreeRef(element);
                this.elements.Insert(elementRef);
                this.elementRefs[element] = elementRef;
            }
            finally
            {
                this.slimLock.ExitWriteLock();
            }
        }
        finally { }
    }
}
