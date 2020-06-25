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

        public Vector3 Position { get; set; }

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
    }
}
