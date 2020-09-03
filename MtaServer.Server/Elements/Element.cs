using MtaServer.Server.Elements.Events;
using System;
using System.Numerics;

namespace MtaServer.Server.Elements
{
    public class Element
    {
        public virtual ElementType ElementType => ElementType.Unknown;
        public Element? Parent { get; set; }

        public uint Id { get; protected set; }
        public byte TimeContext { get; private set; }

        public string Name { get; set; } = "";

        private Vector3 position;
        public Vector3 Position
        {
            get => position;
            set
            {
                PositionChange?.Invoke(new ElementChangeEventArgs<Vector3>(this, value, this.IsSync));
                position = value;
            }
        }

        public Vector3 Rotation { get; set; }
        public Vector3 Velocity { get; set; }
        
        public byte Interior { get; set; }
        public ushort Dimension { get; set; }

        public bool AreCollisionsEnabled { get; set; } = true;
        public bool IsCallPropagationEnabled { get; set; } = false;
        protected bool IsSync { get; private set; } = false;

        public Element()
        {
            this.Id = ElementIdGenerator.GenerateId();

            Created?.Invoke(this);
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

        public void RunAsSync(Action action)
        {
            this.IsSync = true;
            action();
            this.IsSync = false;
        }

        public event Action<ElementChangeEventArgs<Vector3>>? PositionChange;
        public event Action<Element>? Destroyed;

        public static event Action<Element>? Created;
    }
}
