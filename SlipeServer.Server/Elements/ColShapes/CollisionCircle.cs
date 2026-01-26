using SlipeServer.Server.Elements.Events;
using System.Numerics;

namespace SlipeServer.Server.Elements.ColShapes;

public class CollisionCircle : CollisionShape
{
    public Vector2 Position2
    {
        get => new(this.Position.X, this.Position.Y);
        set => this.Position = new Vector3(value.X, value.Y, 0);
    }

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

    public CollisionCircle(Vector2 position, float Radius)
    {
        this.Position2 = position;
        this.Radius = Radius;
    }

    public override bool IsWithin(Vector3 position, byte? interior = null, ushort? dimension = null)
    {
        if ((interior != null && this.Interior != interior) || (dimension != null && this.Dimension != dimension))
            return false;

        return Vector3.Distance(this.Position, new Vector3(position.X, position.Y, 0)) < this.Radius;
    }

    public new CollisionCircle AssociateWith(IMtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public event ElementChangedEventHandler<float>? RadiusChanged;
}
