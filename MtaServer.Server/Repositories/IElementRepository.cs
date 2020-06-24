using System;
using MtaServer.Server.Elements;
using System.Collections.Generic;

namespace MtaServer.Server.Repositories
{
    public interface IElementRepository
    {
        int Count { get; }

        void Add(Element element);
        void Remove(Element element);

        Element Get(uint id);
        IEnumerable<Element> GetAll();
        IEnumerable<TElement> GetByType<TElement>(ElementType elementType);
    }
}
