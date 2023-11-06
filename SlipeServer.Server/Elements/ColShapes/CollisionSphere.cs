using SlipeServer.Server.Elements.Events;
using System.Numerics;

namespace SlipeServer.Server.Elements.ColShapes;

public class CollisionSphere : CollisionShape
{
    private float radiusSquared;
    private float radius;
    public float Radius
    {
        get => this.radius;
        set
        {
            var args = new ElementChangedEventArgs<float>(this, this.radius, value, this.IsSync);
            this.radius = value;
            this.radiusSquared = value * value;
            RadiusChanged?.Invoke(this, args);
        }
    }

    public CollisionSphere(Vector3 position, float Radius)
    {
        this.Position = position;
        this.Radius = Radius;
    }

    public override bool IsWithin(Vector3 position, byte? interior = null, ushort? dimension = null)
    {
        if ((interior != null && this.Interior != interior) || (dimension != null && this.Dimension != dimension))
            return false;

        return Vector3.DistanceSquared(this.Position, position) <= this.radiusSquared;
    }

    public new CollisionSphere AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public event ElementChangedEventHandler<float>? RadiusChanged;
}
