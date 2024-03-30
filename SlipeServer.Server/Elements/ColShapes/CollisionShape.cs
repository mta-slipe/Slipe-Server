using SlipeServer.Server.Elements.Events;
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
    public bool DimensionsMustMatch { get; set; } = true;
    public bool InteriorsMustMatch { get; set; } = true;

    private readonly ConcurrentDictionary<Element, byte> elementsWithin;
    public IEnumerable<Element> ElementsWithin => this.elementsWithin.Select(x => x.Key);

    public CollisionShape()
    {
        this.elementsWithin = new();
    }

    public abstract bool IsWithin(Vector3 position, byte? interior = null, ushort? dimension = null);

    public bool IsWithin(Element element, bool matchInterior = true, bool matchDimension = true)
        => IsWithin(element.Position, matchInterior ? element.Interior : null, matchDimension ? element.Dimension : null);

    public void CheckElementWithin(Element element)
    {
        if (IsWithin(element, this.InteriorsMustMatch, this.DimensionsMustMatch))
        {
            if (!this.elementsWithin.ContainsKey(element))
            {
                this.elementsWithin[element] = 0;
                this.ElementEntered?.Invoke(this, new(this, element));
                element.Destroyed += OnElementDestroyed;
            }
        } else
        {
            if (this.elementsWithin.ContainsKey(element))
            {
                this.elementsWithin.Remove(element, out var _);
                this.ElementLeft?.Invoke(this, new(this, element));
                element.Destroyed -= OnElementDestroyed;
            }
        }
    }

    private void OnElementDestroyed(Element element)
    {
        this.ElementLeft?.Invoke(this, new(this, element));
    }

    public new CollisionShape AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public event ElementEventHandler<CollisionShape, CollisionShapeHitEventArgs>? ElementEntered;
    public event ElementEventHandler<CollisionShape, CollisionShapeLeftEventArgs>? ElementLeft;
}
