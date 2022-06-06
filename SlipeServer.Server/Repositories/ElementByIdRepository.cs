using SlipeServer.Server.Elements;
using SlipeServer.Server.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace SlipeServer.Server.Repositories;

public class ElementByIdRepository : IElementRepository
{
    public int Count => this.elements.Count;

    private readonly Dictionary<uint, Element> elements;
    private readonly ReaderWriterLockSlim slimLock = new();

    public ElementByIdRepository()
    {
        this.elements = new Dictionary<uint, Element>();
    }

    public void Add(Element element)
    {
        this.slimLock.EnterWriteLock();
        this.elements[element.Id] = element;
        this.slimLock.ExitWriteLock();
    }

    public Element? Get(uint id)
    {
        this.slimLock.EnterReadLock();
        var value = this.elements.ContainsKey(id) ? this.elements[id] : null;
        this.slimLock.ExitReadLock();
        return value;
    }

    public void Remove(Element element)
    {
        this.slimLock.EnterWriteLock();
        this.elements.Remove(element.Id);
        this.slimLock.ExitWriteLock();
    }

    public IEnumerable<Element> GetAll()
    {
        this.slimLock.EnterReadLock();
        var value = this.elements.Values.ToArray();
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        this.slimLock.EnterReadLock();
        var value = this.elements.Values.Where(element => element.ElementType == elementType).Cast<TElement>();
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetByType<TElement>() where TElement : Element
    {
        return this.GetByType<TElement>(ElementTypeHelpers.GetElementType<TElement>());
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .Where(kvPair => Vector3.Distance(kvPair.Value.Position, position) < range)
            .Select(kvPair => kvPair.Value);
        this.slimLock.ExitReadLock();
        return value;
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        this.slimLock.EnterReadLock();
        var value = this.elements
            .Where(element => Vector3.Distance(element.Value.Position, position) < range && element.Value.ElementType == elementType)
            .Select(kvPair => kvPair.Value)
            .Cast<TElement>();
        this.slimLock.ExitReadLock();
        return value;
    }
}
