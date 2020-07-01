using System;
using MtaServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MtaServer.Server.Repositories
{
    public class ElementByIdRepository : IElementRepository
    {
        public int Count => elements.Count;

        private readonly Dictionary<uint, Element> elements;

        public ElementByIdRepository()
        {
            this.elements = new Dictionary<uint, Element>();
        }

        public void Add(Element element)
        {
            this.elements[element.Id] = element;
        }

        public Element? Get(uint id)
        {
            return this.elements.ContainsKey(id) ? this.elements[id] : null;
        }

        public void Remove(Element element)
        {
            this.elements.Remove(element.Id);
        }

        public IEnumerable<Element> GetAll()
        {
            return this.elements.Values;
        }

        public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
        {
            return this.elements.Values.Where(element => element.ElementType == elementType).Cast<TElement>();
        }

        public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
        {
            return this.elements
                .Where(kvPair => Vector3.Distance(kvPair.Value.Position, position) < range)
                .Select(kvPair => kvPair.Value);
        }
    }
}
