using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.Repositories
{
    public class ElementByIdRepository : IElementRepository
    {
        public int Count => this.elements.Count;

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
            return this.elements.Values.ToArray();
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

        public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
        {
            return this.elements
                .Where(element => Vector3.Distance(element.Value.Position, position) < range && element.Value.ElementType == elementType)
                .Select(kvPair => kvPair.Value)
                .Cast<TElement>();
        }
    }
}
