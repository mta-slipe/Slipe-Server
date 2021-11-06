using System;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SlipeServer.Server.Elements.Events;
using RBush;

namespace SlipeServer.Server.Repositories
{
    internal class RTreeRef : ISpatialData
    {
        private readonly Envelope envelope;
        public Element Element { get; }

        public RTreeRef(Element element, Vector3 position)
        {
            this.Element = element;
            this.envelope = new Envelope(
                minX: position.X,
                minY: position.Y,
                maxX: position.X,
                maxY: position.Y);
        }

        public RTreeRef(Element element) : this(element, element.Position)
        {

        }

        public ref readonly Envelope Envelope => ref this.envelope;
    }

    public class RTreeElementRepository : IElementRepository
    {
        public int Count => elements.Count;
        private readonly RBush<RTreeRef> elements;
        private readonly Dictionary<Element, RTreeRef> elementRefs;
        private readonly object reinsertLock = new();

        public RTreeElementRepository()
        {
            this.elements = new();
            this.elementRefs = new();
        }

        public void Add(Element element)
        {
            element.PositionChanged += ReInsertElement;
            var elementRef = new RTreeRef(element);
            this.elements.Insert(elementRef);
            this.elementRefs[element] = elementRef;
        }

        public Element? Get(uint id)
        {
            return this.elements
                .Search()
                .Select(x => x.Element)
                .FirstOrDefault(element => element.Id == id);
        }

        public void Remove(Element element)
        {
            element.PositionChanged -= ReInsertElement;
            this.elements.Delete(new(element));
            this.elementRefs.Remove(element);
        }

        public IEnumerable<Element> GetAll()
        {
            return this.elements
                .Search()
                .Select(x => x.Element);
        }

        public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
        {
            return this.elements
                .Search()
                .Select(x => x.Element)
                .Where(element => element.ElementType == elementType)
                .Cast<TElement>();
        }

        public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
        {
            return this.elements
                .Search(new Envelope(position.X - range, position.Y - range, position.X + range, position.Y + range))
                .Select(x => x.Element);
        }

        public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
        {
            return this.elements
                .Search(new Envelope(position.X - range, position.Y - range, position.X + range, position.Y + range))
                .Select(x => x.Element)
                .Where(element => element.ElementType == elementType)
                .Cast<TElement>();
        }

        private void ReInsertElement(Element element, ElementChangedEventArgs<Vector3> args)
        {
            lock (this.elementRefs[element])
            {
                this.elements.Delete(this.elementRefs[element]);

                var elementRef = new RTreeRef(element);
                this.elements.Insert(elementRef);
                this.elementRefs[element] = elementRef;
            }
        }
    }
}
