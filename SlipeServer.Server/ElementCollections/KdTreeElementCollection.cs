using KdTree;
using KdTree.Math;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace SlipeServer.Server.ElementCollections;

public class KdTreeElementCollection : IElementCollection
{
    public int Count => this.elements.Sum(x => x.Value.Count);
    private readonly KdTree<float, List<Element>> elements;
    private readonly ReaderWriterLockSlim slimLock = new();

    public KdTreeElementCollection()
    {
        this.elements = new KdTree<float, List<Element>>(dimensions: 3, new FloatMath());
    }

    public void Add(Element element)
    {
        var position = new float[] { element.Position.X, element.Position.Y, element.Position.Z };
        this.slimLock.EnterWriteLock();
        var elements = this.elements
            .RadialSearch(position, 0)
            .Select(x => x.Value)
            .FirstOrDefault();

        if (elements == null)
            this.elements.Add(position, new()
            {
                element
            });
        else
            elements.Add(element);

        element.PositionChanged += ReInsertElement;
        this.slimLock.ExitWriteLock();
    }

    public Element? Get(uint id)
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .SelectMany(x => x.Value)
            .FirstOrDefault(element => element.Id.Value == id);
        this.slimLock.ExitReadLock();
        return value;
    }

    public void Remove(Element element)
    {
        var position = new float[] { element.Position.X, element.Position.Y, element.Position.Z };

        this.slimLock.EnterWriteLock();
        element.PositionChanged -= ReInsertElement;


        var elements = this.elements
            .RadialSearch(position, 0)
            .Select(x => x.Value)
            .FirstOrDefault();
        if (elements != null)
        {
            if (elements.Count == 1)
                this.elements.RemoveAt(position);
            else
                elements.Remove(element);
        }

        this.slimLock.ExitWriteLock();
    }

    public IEnumerable<Element> GetAll()
    {
        this.slimLock.EnterReadLock();
        var value = this.elements.SelectMany(element => element.Value);
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .SelectMany(element => element.Value)
            .Where(element => element.ElementType == elementType)
            .Cast<TElement>();
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>() where TElement : Element
    {
        var value = this.GetByType<TElement>(ElementTypeHelpers.GetElementType<TElement>());
        return value;
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .RadialSearch(new float[] { position.X, position.Y, position.Z }, range)
            .SelectMany(entry => entry.Value);
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .RadialSearch(new float[] { position.X, position.Y, position.Z }, range)
            .SelectMany(kvPair => kvPair.Value)
            .Where(element => element.ElementType == elementType)
            .Cast<TElement>();
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range) where TElement : Element
    {
        return GetWithinRange<TElement>(position, range, ElementTypeHelpers.GetElementType<TElement>());
    }

    public IEnumerable<Element> GetNearest(Vector3 position, int count)
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .GetNearestNeighbours(new float[] { position.X, position.Y, position.Z }, count)
            .SelectMany(kvPair => kvPair.Value);
        this.slimLock.ExitReadLock();
        return value;
    }

    private void ReInsertElement(Element element, ElementChangedEventArgs<Vector3> args)
    {
        var oldPosition = new float[] { args.OldValue.X, args.OldValue.Y, args.OldValue.Z };
        var newPosition = new float[] { args.NewValue.X, args.NewValue.Y, args.NewValue.Z };

        this.slimLock.EnterWriteLock();

        var oldPositionElements = this.elements
            .RadialSearch(oldPosition, 0)
            .Select(x => x.Value)
            .FirstOrDefault();
        if (oldPositionElements != null)
        {
            if (oldPositionElements.Count == 1)
                this.elements.RemoveAt(oldPosition);
            else
                oldPositionElements.Remove(element);
        }

        var newPositionElements = this.elements
            .RadialSearch(newPosition, 0)
            .Select(x => x.Value)
            .FirstOrDefault();
        if (newPositionElements == null)
            this.elements.Add(newPosition, new()
            {
                element
            });
        else
            newPositionElements.Add(element);

        this.slimLock.ExitWriteLock();
    }
}
