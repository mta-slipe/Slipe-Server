using System;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;
using KdTree;
using KdTree.Math;
using System.Numerics;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Helpers;
using System.Threading;

namespace SlipeServer.Server.Repositories;

public class KdTreeElementRepository : IElementRepository
{
    public int Count => throw new NotImplementedException();
    private readonly KdTree<float, Element> elements;
    private readonly ReaderWriterLockSlim slimLock = new();

    public KdTreeElementRepository()
    {
        this.elements = new KdTree<float, Element>(dimensions: 3, new FloatMath());
    }

    public void Add(Element element)
    {
        this.slimLock.EnterWriteLock();
        element.PositionChanged += ReInsertElement;
        this.elements.Add(new float[] { element.Position.X, element.Position.Y, element.Position.Z }, element);
        this.slimLock.ExitWriteLock();
    }

    public Element? Get(uint id)
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .Select(element => element.Value)
            .FirstOrDefault(element => element.Id == id);
        this.slimLock.ExitReadLock();
        return value;
    }

    public void Remove(Element element)
    {
        this.slimLock.EnterWriteLock();
        element.PositionChanged -= ReInsertElement;
        this.elements.RemoveAt(new float[] { element.Position.X, element.Position.Y, element.Position.Z });
        this.slimLock.ExitWriteLock();
    }

    public IEnumerable<Element> GetAll()
    {
        this.slimLock.EnterReadLock();
        var value = this.elements.Select(element => element.Value);
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .Select(element => element.Value)
            .Where(element => element.ElementType == elementType)
            .Cast<TElement>();
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>() where TElement : Element
    {
        this.slimLock.EnterReadLock();
        var value = this.GetByType<TElement>(ElementTypeHelpers.GetElementType<TElement>());
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .RadialSearch(new float[] { position.X, position.Y, position.Z }, range)
            .Select(entry => entry.Value);
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .RadialSearch(new float[] { position.X, position.Y, position.Z }, range)
            .Where(element => element.Value.ElementType == elementType)
            .Select(kvPair => kvPair.Value)
            .Cast<TElement>();
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<Element> GetNearest(Vector3 position, int count)
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .GetNearestNeighbours(new float[] { position.X, position.Y, position.Z }, count)
            .Select(kvPair => kvPair.Value);
        this.slimLock.ExitReadLock();
        return value;
    }

    private void ReInsertElement(Element element, ElementChangedEventArgs<Vector3> args)
    {
        this.slimLock.EnterWriteLock();
        var neighbour = this.elements
            .RadialSearch(new float[] { args.OldValue.X, args.OldValue.Y, args.OldValue.Z }, 0.25f)
            .SingleOrDefault(x => x.Value == element);
        if (neighbour != null)
            this.elements.RemoveAt(neighbour.Point);
        this.elements.Add(new float[] { args.NewValue.X, args.NewValue.Y, args.NewValue.Z }, args.Source);
        this.slimLock.ExitWriteLock();
    }
}
