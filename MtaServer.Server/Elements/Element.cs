using System;
using System.Numerics;

namespace MtaServer.Server.Elements
{
    public class Element
    {
        private static uint idCounter = 0;
        private static uint GenerateId()
        {
            idCounter++;
            return idCounter;
        }

        public virtual ElementType ElementType => ElementType.Unknown;
        public uint Id { get; protected set; }


        private byte timeContext;
        public byte TimeContext => timeContext;

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

        public Element()
        {
            this.Id = GenerateId();
        }

        public byte GetAndIncrementTimeContext()
        {
            if (++timeContext == 0)
            {
                timeContext++;
            }
            return timeContext;
        }

        public event Action<Element, Vector3> PositionChange;
    }
}
