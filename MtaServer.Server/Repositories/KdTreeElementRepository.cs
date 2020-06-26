using System;
using MtaServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;
using KdTree;
using KdTree.Math;
using System.Numerics;

namespace MtaServer.Server.Repositories
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
            element.PositionChange += ReInsertElement;
        }

        public Element Get(uint id)
        {
            return this.elements
                .Select(element => element.Value)
                .FirstOrDefault(element => element.Id == id);
        }

        public void Remove(Element element)
        {
            this.elements.RemoveAt(new float[] { element.Position.X, element.Position.Y, element.Position.Z });
            element.PositionChange -= ReInsertElement;
        }

        public IEnumerable<Element> GetAll()
        {
            return this.elements.Select(element => element.Value);
        }

        public IEnumerable<TElement> GetByType<TElement>(ElementType elementType)
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

        private void ReInsertElement(Element element, Vector3 newPosition)
        {
            this.Remove(element);
            this.elements.Add(new float[] { newPosition.X, newPosition.Y, newPosition.Z }, element);
        }
    }
}
