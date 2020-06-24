using System;
using MtaServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;

namespace MtaServer.Server.Repositories
{
    public class ElementRepository : IElementRepository
    {
        public int Count => elements.Count;

        private readonly Dictionary<uint, Element> elements;

        public ElementRepository()
        {
            this.elements = new Dictionary<uint, Element>();
        }

        public void Add(Element element)
        {
            this.elements[element.Id] = element;
        }

        public Element Get(uint id)
        {
            return this.elements[id];
        }

        public void Remove(Element element)
        {
            this.elements.Remove(element.Id);
        }

        public IEnumerable<Element> GetAll()
        {
            return this.elements.Values;
        }

        public IEnumerable<TElement> GetByType<TElement>(ElementType elementType)
        {
            return this.elements.Values.Where(element => element.ElementType == elementType).Cast<TElement>();
        }
    }
}
