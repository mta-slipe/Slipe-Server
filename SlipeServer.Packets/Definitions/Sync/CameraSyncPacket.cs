using SlipeServer.Packets.Enums;
using System.Numerics;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Sync;

public class CameraSyncPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_CAMERA_SYNC;
    public override PacketReliability Reliability => PacketReliability.UnreliableSequenced;
    public override PacketPriority Priority => PacketPriority.Medium;

    public byte TimeContext { get; private set; }
    public bool IsFixed { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 LookAt { get; private set; }
    public ElementId TargetId { get; private set; }

    public CameraSyncPacket()
    {

    }

    public CameraSyncPacket(byte timeContext, bool isFixed, Vector3 position, Vector3 lookAt, ElementId targetId)
    {
        this.TimeContext = timeContext;
        this.IsFixed = isFixed;
        this.Position = position;
        this.LookAt = lookAt;
        this.TargetId = targetId;
    }

    public CameraSyncPacket(byte timeContext, Vector3 position, Vector3 lookAt)
        : this(timeContext, true, position, lookAt, ElementId.Zero) { }

    public CameraSyncPacket(byte timeContext, ElementId targetId)
        : this(timeContext, false, Vector3.Zero, Vector3.Zero, targetId) { }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        this.TimeContext = reader.GetByte();
        this.IsFixed = reader.GetBit();

        if (this.IsFixed)
        {
            if (reader.Size - reader.Counter > 3 * 24)
                this.Position = reader.GetVector3WithZAsFloat(14, 10);
            if (reader.Size - reader.Counter > 3 * 24)
                this.LookAt = reader.GetVector3WithZAsFloat(14, 10);
            this.TargetId = ElementId.Zero;
        } else
        {
            this.TargetId = reader.GetElementId();
        }
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.TimeContext);
        builder.Write(this.IsFixed);

        if (this.IsFixed)
        {
            builder.WriteVector3WithZAsFloat(this.Position, 14, 10);
            builder.WriteVector3WithZAsFloat(this.LookAt, 14, 10);
        } else
        {
            builder.Write(this.TargetId);
        }

        return builder.Build();
    }

    public override void Reset()
    {
        this.Position = Vector3.Zero;
        this.LookAt = Vector3.Zero;
    }
}
