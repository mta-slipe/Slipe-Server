using System;
using MtaServer.Server.Elements;
using System.Collections.Generic;
using System.Numerics;

namespace MtaServer.Server.Repositories
{
    public interface IElementRepository
    {
        int Count { get; }

        void Add(Element element);
        void Remove(Element element);

        Element? Get(uint id);
        IEnumerable<Element> GetAll();
        IEnumerable<Element> GetWithinRange(Vector3 position, float range);
        IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element;
        IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement: Element;
    }
}
