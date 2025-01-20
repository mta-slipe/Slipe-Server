using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Ped;

public sealed class PedStartSyncPacket : Packet
{
    public override PacketId PacketId { get; } = PacketId.PACKET_ID_PED_STARTSYNC;
    public override PacketReliability Reliability { get; } = PacketReliability.ReliableSequenced;
    public override PacketPriority Priority { get; } = PacketPriority.High;

    public ElementId SourceElementId { get; set; }
    public Vector3 Position { get; set; }
    public float Rotation { get; set; }
    public Vector3 Velocity { get; set; }
    public float Health { get; set; }
    public float Armor { get; set; }

    public PedStartSyncPacket(ElementId sourceElementId, Vector3 position, float rotation, Vector3 velocity, float health, float armor)
    {
        this.SourceElementId = sourceElementId;
        this.Position = position;
        this.Rotation = rotation;
        this.Velocity = velocity;
        this.Health = health;
        this.Armor = armor;
    }

    public PedStartSyncPacket(ElementId sourceElementId)
    {
        this.SourceElementId = sourceElementId;
    }

    public PedStartSyncPacket()
    {

    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.SourceElementId);
        builder.Write(this.Position);
        builder.Write(this.Rotation);
        builder.Write(this.Velocity);
        builder.Write(this.Health);
        builder.Write(this.Armor);

        return builder.Build();
    }

    public override void Read(byte[] bytes)
    {

    }
}
