using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Server.Elements.ColShapes
{
    public abstract class CollisionShape : Element
    {
        public override ElementType ElementType => ElementType.Colshape;

        public bool IsEnabled { get; set; } = true;
        public bool AutoCallEvent { get; set; } = true;

        private readonly HashSet<Element> elementsWithin;
        public IEnumerable<Element> ElementsWithin => this.elementsWithin.AsEnumerable();

        public CollisionShape()
        {
            this.elementsWithin = new HashSet<Element>();
        }

        public abstract bool IsWithin(Vector3 position);

        public bool IsWithin(Element element) => IsWithin(element.Position);

        public void CheckElementWithin(Element element)
        {
            if (IsWithin(element))
            {
                if (!this.elementsWithin.Contains(element))
                {
                    this.elementsWithin.Add(element);
                    element.RunAsSync(() =>
                    {
                        this.ElementEntered?.Invoke(element);
                    }, false);
                    element.Destroyed += OnElementDestroyed;
                }
            } else
            {
                if (this.elementsWithin.Contains(element))
                {
                    this.elementsWithin.Remove(element);
                    element.RunAsSync(() => {
                        this.ElementLeft?.Invoke(element);
                    }, false);
                    element.Destroyed -= OnElementDestroyed;
                }
            }
        }

        private void OnElementDestroyed(Element element)
        {
            this.ElementLeft?.Invoke(element);
        }

        public new CollisionShape AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        public event Action<Element>? ElementEntered;
        public event Action<Element>? ElementLeft;
    }
}
