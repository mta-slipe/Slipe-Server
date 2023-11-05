using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.Elements.ColShapes;

/// <summary>
/// Collision shape element base class.
/// Collision shapes are invisible elements, that trigger an event when a player enters the shape.
/// </summary>
public abstract class CollisionShape : Element
{
    public override ElementType ElementType => ElementType.Colshape;

    public bool IsEnabled { get; set; } = true;
    public bool AutoCallEvent { get; set; } = true;

    private readonly ConcurrentDictionary<Element, byte> elementsWithin;
    public IEnumerable<Element> ElementsWithin => this.elementsWithin.Select(x => x.Key);

    public CollisionShape()
    {
        this.elementsWithin = new();
    }

    public abstract bool IsWithin(Vector3 position, byte? interior = null, ushort? dimension = null);

    public bool IsWithin(Element element, bool matchInterior = true, bool matchDimension = true)
        => IsWithin(element.Position, matchInterior ? element.Interior : null, matchDimension ? element.Dimension : null);

    public void CheckElementWithin(Element element, bool matchInterior = true, bool matchDimension = true)
    {
        if (IsWithin(element, matchInterior, matchDimension))
        {
            if (!this.elementsWithin.ContainsKey(element))
            {
                this.elementsWithin[element] = 0;
                this.ElementEntered?.Invoke(element);
                element.Destroyed += OnElementDestroyed;
            }
        } else
        {
            if (this.elementsWithin.ContainsKey(element))
            {
                this.elementsWithin.Remove(element, out var _);
                this.ElementLeft?.Invoke(element);
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
        base.AssociateWith(server);
        return this;
    }

    public event Action<Element>? ElementEntered;
    public event Action<Element>? ElementLeft;
}
