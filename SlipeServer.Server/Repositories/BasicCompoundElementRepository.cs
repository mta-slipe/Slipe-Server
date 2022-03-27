using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Server.Repositories;

public class BasicCompoundElementRepository : IElementRepository
{
    public int Count => this.flatElementRepository.Count;

    private readonly FlatElementRepository flatElementRepository;
    private readonly ElementByIdRepository elementByIdRepository;
    private readonly ElementByTypeRepository elementByTypeRepository;
    private readonly KdTreeElementRepository kdTreeElementRepository;

    public BasicCompoundElementRepository()
    {
        this.flatElementRepository = new FlatElementRepository();
        this.elementByIdRepository = new ElementByIdRepository();
        this.elementByTypeRepository = new ElementByTypeRepository();
        this.kdTreeElementRepository = new KdTreeElementRepository();
    }

    public void Add(Element element)
    {
        this.flatElementRepository.Add(element);
        this.elementByIdRepository.Add(element);
        this.elementByTypeRepository.Add(element);
        this.kdTreeElementRepository.Add(element);
    }

    public void Remove(Element element)
    {
        this.flatElementRepository.Remove(element);
        this.elementByIdRepository.Remove(element);
        this.elementByTypeRepository.Remove(element);
        this.kdTreeElementRepository.Remove(element);
    }

    public Element? Get(uint id)
    {
        return this.elementByIdRepository.Get(id);
    }

    public IEnumerable<Element> GetAll()
    {
        return this.flatElementRepository.GetAll();
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        return this.elementByTypeRepository.GetByType<TElement>(elementType);
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        return this.kdTreeElementRepository.GetWithinRange(position, range);
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        return this.kdTreeElementRepository.GetWithinRange<TElement>(position, range, elementType);
    }
}
