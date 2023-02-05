using SlipeServer.Packets.Enums;
using System.Numerics;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Sync;

public class ProjectileSyncPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PROJECTILE;
    public override PacketReliability Reliability => PacketReliability.Reliable;
    public override PacketPriority Priority => PacketPriority.Medium;

    public byte WeaponType { get; set; }
    public ElementId OriginId { get; set; }
    public Vector3 VecOrigin { get; set; }
    public float Force { get; set; }
    public byte HasTarget { get; set; }
    public ElementId TargetId { get; set; }
    public Vector3 VecTarget { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 MoveSpeed { get; set; }
    public ushort Model { get; set; }
    public ElementId SourceElement { get; set; }
    public ushort? Latency { get; }


    public ProjectileSyncPacket()
    {

    }

    public ProjectileSyncPacket(Vector3 origin, Vector3 direction, ElementId sourceElement, byte weaponType, ushort model)
    {
        this.Model = model;
        this.SourceElement = sourceElement;
        this.WeaponType = weaponType;
        this.VecOrigin = origin;
        this.MoveSpeed = direction;
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        bool hasOrigin = reader.GetBit();
        if (hasOrigin)
            this.OriginId = reader.GetElementId();

        this.VecOrigin = reader.GetVector3WithZAsFloat();
        this.WeaponType = reader.GetWeaponType();
        this.Model = reader.GetUint16();

        switch (this.WeaponType)
        {
            case 16: // WEAPONTYPE_GRENADE
            case 17: // WEAPONTYPE_TEARGAS
            case 18: // WEAPONTYPE_MOLOTOV
            case 39: // WEAPONTYPE_REMOTE_SATCHEL_CHARGE
                this.Force = reader.GetFloatFromBits(24, -128, 128);
                this.MoveSpeed = reader.GetVelocityVector();
                break;
            case 19: // WEAPONTYPE_ROCKET
            case 20: // WEAPONTYPE_ROCKET_HS
                bool hasTarget = reader.GetBit();
                if (hasTarget)
                    this.TargetId = (ElementId)reader.GetUint32();

                this.MoveSpeed = reader.GetVelocityVector();
                this.Rotation = reader.GetVector3();

                break;
        }
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.SourceElement.Value != 0);
        if (this.SourceElement.Value != 0)
        {
            builder.Write(this.SourceElement);
            builder.WriteCompressed(this.Latency ?? 0);
        }

        builder.Write(this.OriginId.Value != 0);
        if (this.OriginId.Value != 0)
        {
            builder.Write(this.OriginId);
        }

        builder.WriteVector3WithZAsFloat(this.VecOrigin);
        builder.WriteWeaponType(this.WeaponType);
        builder.Write(this.Model);

        switch (this.WeaponType)
        {
            case 16:            // WEAPONTYPE_GRENADE
            case 17:            // WEAPONTYPE_TEARGAS
            case 18:            // WEAPONTYPE_MOLOTOV
            case 39:            // WEAPONTYPE_REMOTE_SATCHEL_CHARGE
                builder.WriteFloat(this.Force, 7, 17);
                builder.WriteVelocityVector(this.MoveSpeed);
                break;
            case 19:            // WEAPONTYPE_ROCKET
            case 20:            // WEAPONTYPE_ROCKET_HS
                builder.Write(this.TargetId.Value != 0);
                if (this.TargetId.Value != 0)
                {
                    builder.Write(this.TargetId);
                }

                builder.WriteVelocityVector(this.MoveSpeed);
                builder.Write(this.Rotation);
                break;
        }

        return builder.Build();
    }

    public override void Reset()
    {
        this.Force = 0;
        this.MoveSpeed = Vector3.Zero;
        this.Rotation = Vector3.Zero;
        this.TargetId = ElementId.Zero;
    }
}
