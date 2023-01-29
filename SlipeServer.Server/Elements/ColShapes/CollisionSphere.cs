using SlipeServer.Server.Elements.Events;
using System.Numerics;

namespace SlipeServer.Server.Elements.ColShapes;

public class CollisionSphere : CollisionShape
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

    public CollisionSphere(Vector3 position, float Radius)
    {
        this.Position = position;
        this.Radius = Radius;
    }

    public override bool IsWithin(Vector3 position)
    {
        return Vector3.Distance(this.Position, position) < this.Radius;
    }

    public new CollisionSphere AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public event ElementChangedEventHandler<float>? RadiusChanged;
}
