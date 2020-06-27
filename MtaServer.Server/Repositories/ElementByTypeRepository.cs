using System;
using MtaServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MtaServer.Server.Repositories
{
    public class ElementByTypeRepository : IElementRepository
    {
        public int Count => elements.Count;

        private readonly Dictionary<ElementType, List<Element>> elements;

        public ElementByTypeRepository()
        {
            this.elements = new Dictionary<ElementType, List<Element>>();
        }

        public void Add(Element element)
        {
            if (!this.elements.ContainsKey(element.ElementType)){
                this.elements[element.ElementType] = new List<Element>();
            }
            this.elements[element.ElementType].Add(element);
        }

        public Element Get(uint id)
        {
            foreach(var list in this.elements.Values)
            {
                var element = list.FirstOrDefault(element => element.Id == id);
                if (element != null)
                {
                    return element;
                }
            }
            return null;
        }

        public void Remove(Element element)
        {
            foreach (var list in this.elements.Values)
            {
                list.Remove(element);
            }
        }

        public IEnumerable<Element> GetAll()
        {
            return this.elements.SelectMany(kvPair => kvPair.Value);
        }

        public IEnumerable<TElement> GetByType<TElement>(ElementType elementType)
        {
            return this.elements.ContainsKey(elementType) ? this.elements[elementType].Cast<TElement>() : new TElement[0];
        }

        public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
        {
            return this.elements
                .SelectMany(kvPair => kvPair.Value)
                .Where(element => Vector3.Distance(element.Position, position) < range);
        }
    }
}
