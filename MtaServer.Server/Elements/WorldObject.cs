using MtaServer.Packets.Definitions.Entities.Structs;
using System;
using System.Numerics;

namespace MtaServer.Server.Elements
{
    public class WorldObject : Element
    {
        public override ElementType ElementType => ElementType.Object;

        public ushort Model { get; set; }
        public byte Alpha { get; set; } = 255;
        public bool IsLowLod { get; set; } = false;
        public WorldObject? LowLodElement { get; set; }
        public bool DoubleSided { get; set; } = false;
        public bool IsVisibleInAllDimensions { get; set; } = true;
        public PositionRotationAnimation? Movement { get; set; }
        public Vector3 Scale { get; set; } = Vector3.One;
        public bool IsFrozen { get; set; } = false;
        public float Health { get; set; } = 1000;

        public WorldObject(ushort model, Vector3 position)
        {
            this.Model = model;
            this.Position = position;
        }

        public new WorldObject AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
