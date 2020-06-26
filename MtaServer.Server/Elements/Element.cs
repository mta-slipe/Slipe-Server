using System;
using System.Numerics;

namespace MtaServer.Server.Elements
{
    public class Element
    {
        public virtual ElementType ElementType => ElementType.Unknown;
        public uint Id { get; protected set; }
        public byte TimeContext { get; private set; }

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

        public Element()
        {
            this.Id = ElementIdGenerator.GenerateId();
        }

        public byte GetAndIncrementTimeContext()
        {
            if (++TimeContext == 0)
            {
                TimeContext++;
            }
            return TimeContext;
        }

        public event Action<Element, Vector3> PositionChange;
    }
}
