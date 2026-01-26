using SlipeServer.Server.Elements.Events;
using System.Numerics;

namespace SlipeServer.Server.Elements.ColShapes;

public class CollisionRectangle : CollisionShape
{
    public Vector2 Position2
    {
        get => new(this.Position.X, this.Position.Y);
        set => this.Position = new Vector3(value.X, value.Y, 0);
    }

    private Vector2 dimensions;
    public Vector2 Dimensions
    {
        get => this.dimensions;
        set
        {
            var args = new ElementChangedEventArgs<Vector2>(this, this.dimensions, value, this.IsSync);
            this.dimensions = value;
            DimensionsChanged?.Invoke(this, args);
        }
    }

    public CollisionRectangle(Vector2 position, Vector2 dimensions)
    {
        this.Position2 = position;
        this.dimensions = dimensions;
    }

    public override bool IsWithin(Vector3 position, byte? interior = null, ushort? dimension = null)
    {
        if ((interior != null && this.Interior != interior) || (dimension != null && this.Dimension != dimension))
            return false;

        Vector2 bounds = this.Position2 + this.Dimensions;

        return
            position.X > this.Position.X && position.X < bounds.X &&
            position.Y > this.Position.Y && position.Y < bounds.Y;
    }

    public new CollisionRectangle AssociateWith(IMtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public event ElementChangedEventHandler<Vector2>? DimensionsChanged;
}
