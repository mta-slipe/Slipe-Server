﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Ped;

public sealed class PedTaskPacket : Packet
{
    public override PacketId PacketId { get; } = PacketId.PACKET_ID_PED_TASK;
    public override PacketReliability Reliability { get; } = PacketReliability.Reliable;
    public override PacketPriority Priority { get; } = PacketPriority.High;

    public ElementId SourceElementId { get; set; }
    public ushort TaskType { get; set; }
    public ElementId AttackerId { get; set; }
    public byte HitBodyPart { get; set; }
    public byte HitBodySize { get; set; }
    public byte WeaponId { get; set; }


    public PedTaskPacket(ElementId sourceElementId, byte weaponId, byte hitBodySize, byte hitBodyPart, ElementId attackerId, ushort taskType)
    {
        this.SourceElementId = sourceElementId;
        this.WeaponId = weaponId;
        this.HitBodySize = hitBodySize;
        this.HitBodyPart = hitBodyPart;
        this.AttackerId = attackerId;
        this.TaskType = taskType;
    }

    public PedTaskPacket()
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.SourceElementId);

        builder.Write(this.TaskType);
        builder.Write(this.AttackerId);
        builder.Write(this.HitBodyPart);
        builder.Write(this.HitBodySize);
        builder.Write(this.WeaponId);

        return builder.Build();
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        this.TaskType = reader.GetByte();
        this.AttackerId = reader.GetElementId();
        this.HitBodyPart = reader.GetByte();
        this.HitBodySize = reader.GetByte();
        this.WeaponId = reader.GetByte();
    }
}
