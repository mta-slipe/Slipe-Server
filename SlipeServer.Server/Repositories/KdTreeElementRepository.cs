using System;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;
using KdTree;
using KdTree.Math;
using System.Numerics;
using SlipeServer.Server.Elements.Events;

namespace SlipeServer.Server.Repositories
{
    public class KdTreeElementRepository : IElementRepository
    {
        public int Count => throw new NotImplementedException();
        private readonly KdTree<float, Element> elements;

        public KdTreeElementRepository()
        {
            this.elements = new KdTree<float, Element>(dimensions: 3, new FloatMath());
        }

        public void Add(Element element)
        {
            this.elements.Add(new float[] { element.Position.X, element.Position.Y, element.Position.Z }, element);
            element.PositionChanged += ReInsertElement;
        }

        public Element? Get(uint id)
        {
            return this.elements
                .Select(element => element.Value)
                .FirstOrDefault(element => element.Id == id);
        }

        public void Remove(Element element)
        {
            this.elements.RemoveAt(new float[] { element.Position.X, element.Position.Y, element.Position.Z });
            element.PositionChanged -= ReInsertElement;
        }

        public IEnumerable<Element> GetAll()
        {
            return this.elements.Select(element => element.Value);
        }

        public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
        {
            return this.elements
                .Select(element => element.Value)
                .Where(element => element.ElementType == elementType)
                .Cast<TElement>();
        }

        public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
        {
            return this.elements
                .RadialSearch(new float[] { position.X, position.Y, position.Z }, range)
                .Select(entry => entry.Value);
        }

        public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
        {
            return this.elements
                .RadialSearch(new float[] { position.X, position.Y, position.Z }, range)
                .Where(element => element.Value.ElementType == elementType)
                .Select(kvPair => kvPair.Value)
                .Cast<TElement>();
        }

        private void ReInsertElement(Element sender, ElementChangedEventArgs<Vector3> args)
        {
            this.elements.RemoveAt(new float[] { sender.Position.X, sender.Position.Y, sender.Position.Z });
            this.elements.Add(new float[] { args.NewValue.X, args.NewValue.Y, args.NewValue.Z }, args.Source);
        }
    }
}
