using SlipeServer.Server.Elements;
using SlipeServer.Server.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace SlipeServer.Server.Repositories;

public class FlatElementRepository : IElementRepository
{
    public int Count => this.elements.Count;

    private readonly List<Element> elements;
    private readonly ReaderWriterLockSlim slimLock = new();

    public FlatElementRepository()
    {
        this.elements = new List<Element>();
    }

    public void Add(Element element)
    {
        this.slimLock.EnterWriteLock();
        this.elements.Add(element);
        this.slimLock.ExitWriteLock();
    }

    public Element? Get(uint id)
    {
        this.slimLock.EnterReadLock();
        var value = this.elements.FirstOrDefault(element => element.Id == id);
        this.slimLock.ExitReadLock();
        return value;
    }

    public void Remove(Element element)
    {
        this.slimLock.EnterWriteLock();
        this.elements.Remove(element);
        this.slimLock.ExitWriteLock();
    }

    public IEnumerable<Element> GetAll()
    {
        this.slimLock.EnterReadLock();
        var value = this.elements.Select(element => element);
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        this.slimLock.EnterReadLock();
        var value = this.elements.Where(element => element.ElementType == elementType).Cast<TElement>();
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
        var value = this.elements.Where(element => Vector3.Distance(element.Position, position) < range);
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .Where(element => Vector3.Distance(element.Position, position) < range && element.ElementType == elementType)
            .Cast<TElement>();
        this.slimLock.ExitReadLock();
        return value;
    }
}
