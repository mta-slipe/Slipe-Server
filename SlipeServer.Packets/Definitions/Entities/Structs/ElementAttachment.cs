using System.Numerics;

namespace SlipeServer.Packets.Definitions.Entities.Structs;

public struct ElementAttachment
{
    public uint ElementId { get; set; }
    public Vector3 AttachmentPosition { get; set; }
    public Vector3 AttachmentRotation { get; set; }
}
