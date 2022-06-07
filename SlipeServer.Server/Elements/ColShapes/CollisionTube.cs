using SlipeServer.Server.Elements.Events;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Server.Elements.ColShapes;

public class CollisionTube : CollisionShape
{
    private float radius;
    public float Radius
    {
        get => this.radius;
        set
        {
            var args = new ElementChangedEventArgs<float>(this, this.radius, value, this.IsSync);
            this.radius = value;
            RadiusChanged?.Invoke(this, args);
        }
    }

    private float height;
    public float Height
    {
        get => this.height;
        set
        {
            var args = new ElementChangedEventArgs<float>(this, this.height, value, this.IsSync);
            this.height = value;
            HeightChanged?.Invoke(this, args);
        }
    }


    public CollisionTube(Vector3 position, float Radius, float Height)
    {
        this.Position = position;
        this.Radius = Radius;
        this.Height = Height;
    }

    public override bool IsWithin(Vector3 position)
    {
        return Vector3.Distance(this.Position, new Vector3(position.X, position.Y, 0)) < this.Radius &&
            position.Z > this.Position.Z && position.Z < this.Position.Z + this.Height;
    }

    public new CollisionTube AssociateWith(MtaServer server)
    {
        return server.AssociateElement(this);
    }

    public event ElementChangedEventHandler<float>? RadiusChanged;
    public event ElementChangedEventHandler<float>? HeightChanged;
}
