﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Fire;

public sealed class FirePacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_FIRE;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public Vector3 Position { get; set; }
    public float Size { get; set; }
    public ElementId? SourceElementId { get; set; }
    public ushort? Latency { get; }

    public FirePacket()
    {

    }

    public FirePacket(Vector3 position, float size, ElementId? sourceElementId = null, ushort? latency = null)
    {
        this.Position = position;
        this.Size = size;
        this.SourceElementId = sourceElementId;
        this.Latency = latency;
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        if (this.SourceElementId.HasValue)
        {
            builder.Write(this.SourceElementId.Value);
            builder.WriteCompressed(this.Latency ?? 0);
        } else
        {
            builder.Write(ElementId.Zero);
            builder.WriteCompressed((ushort)0);
        }
        builder.Write(this.Position);
        builder.Write(this.Size);

        return builder.Build();
    }

    public override void Read(byte[] bytes)
    {

    }
}
