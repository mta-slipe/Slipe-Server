using SlipeServer.Server.Elements.Events;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Server.Elements.ColShapes
{
    public class CollisionCuboid : CollisionShape
    {
        private Vector3 dimensions;
        public Vector3 Dimensions
        {
            get => this.dimensions; set
            {
                var args = new ElementChangedEventArgs<Vector3>(this, this.dimensions, value, this.IsSync);
                this.dimensions = value;
                DimensionsChanged?.Invoke(this, args);
            }
        }


        public CollisionCuboid(Vector3 position, Vector3 dimensions)
        {
            this.Position = position;
            this.dimensions = dimensions;
        }

        public override bool IsWithin(Vector3 position)
        {
            Vector3 bounds = this.Position + this.Dimensions;

            return
                position.X > this.Position.X && position.X < bounds.X &&
                position.Y > this.Position.Y && position.Y < bounds.Y &&
                position.Z > this.Position.Z && position.Z < bounds.Z;
        }

        public new CollisionCuboid AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        public event ElementChangedEventHandler<Vector3>? DimensionsChanged;
    }
}
