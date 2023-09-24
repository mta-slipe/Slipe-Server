using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets;

public abstract class Packet
{
    public abstract PacketId PacketId { get; }
    public abstract PacketReliability Reliability { get; }
    public abstract PacketPriority Priority { get; }

    public virtual byte[] Write() => Array.Empty<byte>();

    public virtual void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public virtual void Reset() { }
}
