﻿using System;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SlipeServer.Server.Helpers;
using System.Threading;

namespace SlipeServer.Server.ElementCollections;

public class ElementByTypeCollection : IElementCollection
{
    public int Count => this.elements.Select(x => x.Value.Count).Sum();

    private readonly Dictionary<ElementType, List<Element>> elements;
    private readonly ReaderWriterLockSlim slimLock = new();

    public ElementByTypeCollection()
    {
        this.elements = [];
    }

    public void Add(Element element)
    {
        this.slimLock.EnterWriteLock();
        if (!this.elements.ContainsKey(element.ElementType))
        {
            this.elements[element.ElementType] = [];
        }
        this.elements[element.ElementType].Add(element);
        this.slimLock.ExitWriteLock();
    }

    public Element? Get(uint id)
    {
        this.slimLock.EnterReadLock();
        foreach (var list in this.elements.Values)
        {
            var element = list.FirstOrDefault(element => element.Id.Value == id);
            if (element != null)
            {
                this.slimLock.ExitReadLock();
                return element;
            }
        }
        this.slimLock.ExitReadLock();
        return null;
    }

    public void Remove(Element element)
    {
        if (this.elements.ContainsKey(element.ElementType))
        {
            this.slimLock.EnterWriteLock();
            this.elements[element.ElementType].Remove(element);
            this.slimLock.ExitWriteLock();
        }
    }

    public IEnumerable<Element> GetAll()
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .SelectMany(kvPair => kvPair.Value)
            .ToArray();
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        this.slimLock.EnterReadLock();
        var value = this.elements.ContainsKey(elementType) ? this.elements[elementType].Cast<TElement>().ToArray() : Array.Empty<TElement>();
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
            .SelectMany(kvPair => kvPair.Value)
            .Where(element => Vector3.Distance(element.Position, position) < range)
            .ToArray();
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        this.slimLock.EnterReadLock();
        var value = this.elements.ContainsKey(elementType) ?
            this.elements[elementType].Cast<TElement>()
                .Where(element => Vector3.Distance(element.Position, position) < range)
                .ToArray() :
            Array.Empty<TElement>();
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range) where TElement : Element
    {
        return GetWithinRange<TElement>(position, range, ElementTypeHelpers.GetElementType<TElement>());
    }
}
