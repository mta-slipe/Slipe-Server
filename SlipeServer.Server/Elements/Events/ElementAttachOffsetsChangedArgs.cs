using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements.Events
{
    public class ElementAttachOffsetsChangedArgs : EventArgs
    {
        public Element Source { get; set; }
        public Vector3 OffsetPosition { get; set; }
        public Vector3 OffsetRotation { get; set; }

        public ElementAttachOffsetsChangedArgs(Element source, Vector3 offsetPosition, Vector3 offsetRotation)
        {
            this.Source = source;
            this.OffsetPosition = offsetPosition;
            this.OffsetRotation = offsetRotation;
        }
    }
}
