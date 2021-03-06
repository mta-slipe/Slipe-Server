using System;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.Repositories
{
    public class FlatElementRepository : IElementRepository
    {
        public int Count => elements.Count;

        private readonly List<Element> elements;

        public FlatElementRepository()
        {
            this.elements = new List<Element>();
        }

        public void Add(Element element)
        {
            this.elements.Add(element);
        }

        public Element? Get(uint id)
        {
            return this.elements.FirstOrDefault(element => element.Id == id);
        }

        public void Remove(Element element)
        {
            this.elements.Remove(element);
        }

        public IEnumerable<Element> GetAll()
        {
            return this.elements.Select(element => element);
        }

        public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
        {
            return this.elements.Where(element => element.ElementType == elementType).Cast<TElement>();
        }

        public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
        {
            return this.elements.Where(element => Vector3.Distance(element.Position, position) < range);
        }

        public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
        {
            return this.elements
                .Where(element => Vector3.Distance(element.Position, position) < range && element.ElementType == elementType)
                .Cast<TElement>();
        }
    }
}
