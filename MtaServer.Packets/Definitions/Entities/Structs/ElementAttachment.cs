using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Entities.Structs
{
    public struct ElementAttachment
    {
        public uint ElementId { get; set; }
        public Vector3 AttachmentPosition { get; set; }
        public Vector3 AttachmentRotation { get; set; }
    }
}
