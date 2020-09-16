using System;
using System.Numerics;

namespace MtaServer.Server.Elements
{
    public class Element
    {
        public virtual ElementType ElementType => ElementType.Unknown;
        public Element? Parent { get; set; }

        public uint Id { get; set; }
        public byte TimeContext { get; private set; }

        public string Name { get; set; } = "";

        private Vector3 position;
        public Vector3 Position
        {
            get => position;
            set
            {
                PositionChange?.Invoke(this, value);
                position = value;
            }
        }

        public Vector3 Rotation { get; set; }
        public Vector3 Velocity { get; set; }
        
        public byte Interior { get; set; }
        public ushort Dimension { get; set; }

        public bool AreCollisionsEnabled { get; set; } = true;
        public bool IsCallPropagationEnabled { get; set; } = false;

        public Element()
        {

        }

        public Element(Element parent) : this()
        {
            Parent = parent;
        }

        public byte GetAndIncrementTimeContext()
        {
            if (++TimeContext == 0)
            {
                TimeContext++;
            }
            return TimeContext;
        }

        public void Destroy()
        {
            this.Destroyed?.Invoke(this);
        }

        public Element AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        public event Action<Element, Vector3>? PositionChange;
        public event Action<Element>? Destroyed;
    }
}
