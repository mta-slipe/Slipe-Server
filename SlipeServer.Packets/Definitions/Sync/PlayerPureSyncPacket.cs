﻿using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structures;
using System;
using System.Numerics;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Constants;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Sync;

public sealed class PlayerPureSyncPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_PURESYNC;
    public override PacketReliability Reliability => PacketReliability.UnreliableSequenced;
    public override PacketPriority Priority => PacketPriority.Medium;

    public ElementId PlayerId { get; set; }
    public byte TimeContext { get; set; }
    public ushort Latency { get; set; }
    public FullKeySyncStructure KeySync { get; set; } = new FullKeySyncStructure();
    public PlayerPureSyncFlagsStructure SyncFlags { get; set; } = new PlayerPureSyncFlagsStructure();
    public ElementId ContactElementId { get; set; }
    public Vector3 Position { get; set; }
    public float Rotation { get; set; }
    public Vector3 Velocity { get; set; }
    public float Health { get; set; }
    public float Armor { get; set; }
    public float CameraRotation { get; set; }
    public CameraOrientationStructure CameraOrientation { get; set; } = new CameraOrientationStructure(Vector3.Zero);
    public byte WeaponType { get; set; }
    public byte WeaponSlot { get; set; }
    public ushort TotalAmmo { get; set; }
    public ushort AmmoInClip { get; set; }
    public float Arm { get; set; }
    public Vector3 AimOrigin { get; set; }
    public Vector3 AimDirection { get; set; }

    public bool IsDamageChanged { get; set; }
    public ElementId DamagerId { get; set; }
    public byte DamageType { get; set; }
    public byte DamageBodypart { get; set; }

    public PlayerPureSyncPacket()
    {

    }

    public PlayerPureSyncPacket(byte timeContext)
    {
        this.TimeContext = timeContext;
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        this.TimeContext = reader.GetByte();

        this.KeySync.Read(reader);
        this.SyncFlags.Read(reader);
        if (this.SyncFlags.HasContact)
        {
            this.ContactElementId = reader.GetElementId();
        }

        this.Position = reader.GetVector3WithZAsFloat();
        this.Rotation = reader.GetFloatFromBits(16, -MathF.PI, MathF.PI);

        if (this.SyncFlags.IsSyncingVelocity)
        {
            this.Velocity = reader.GetVelocityVector();
        }

        this.Health = reader.GetPlayerHealth();
        this.Armor = reader.GetPlayerArmor();

        this.CameraRotation = reader.GetFloatFromBits(12, -MathF.PI, MathF.PI);
        this.CameraOrientation = new CameraOrientationStructure(this.Position);
        this.CameraOrientation.Read(reader);

        if (this.SyncFlags.HasAWeapon)
        {
            this.WeaponType = reader.GetByte();
            this.WeaponSlot = reader.GetByteCapped(4);

            if (WeaponConstants.SlotsWithAmmo.Contains(this.WeaponSlot))
            {
                this.TotalAmmo = reader.GetAmmo();
                this.AmmoInClip = reader.GetAmmo();

                this.Arm = ((reader.GetUint16()) * MathF.PI / 180f) / 90.0f;

                // if player is aiming
                if (this.KeySync.RightShoulder1 || this.KeySync.ButtonCircle)
                {
                    this.AimOrigin = reader.GetVector3();
                    this.AimDirection = reader.GetNormalizedVector();
                }
            }
        }

        this.IsDamageChanged = reader.GetBit();
        if (this.IsDamageChanged)
        {
            this.DamagerId = reader.GetElementId();
            this.DamageType = reader.GetByteCapped(6);
            this.DamageBodypart = reader.GetBodyPart();
        }
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.PlayerId);
        builder.Write(this.TimeContext);
        builder.WriteCompressed(this.Latency);

        this.KeySync.Write(builder);
        this.SyncFlags.Write(builder);
        if (this.SyncFlags.HasContact)
        {
            builder.Write(this.ContactElementId);
        }

        builder.WriteVector3WithZAsFloat(this.Position);
        builder.WriteFloatFromBits(this.Rotation, 16, -MathF.PI, MathF.PI, false);

        if (this.SyncFlags.IsSyncingVelocity)
        {
            builder.WriteVelocityVector(this.Velocity);
        }

        builder.WritePlayerHealth(this.Health);
        builder.WritePlayerArmor(this.Armor);

        builder.WriteFloatFromBits(this.CameraRotation, 12, -MathF.PI, MathF.PI, true, false);

        if (this.SyncFlags.HasAWeapon)
        {
            builder.WriteWeaponSlot(this.WeaponSlot);

            if (WeaponConstants.SlotsWithAmmo.Contains(this.WeaponSlot))
            {
                builder.WriteAmmo(null, this.AmmoInClip);

                builder.Write((short)(this.Arm * 90f * 180f / MathF.PI));

                // if player is aiming
                if (this.KeySync.RightShoulder1 || this.KeySync.ButtonCircle)
                {
                    builder.Write(this.AimOrigin);
                    builder.WriteNormalizedVector(this.AimDirection);
                }
            }
        }

        return builder.Build();
    }

    public override void Reset()
    {
        this.ContactElementId = ElementId.Zero;
        this.Velocity = Vector3.Zero;

        this.WeaponType = 0;
        this.WeaponSlot = 0;

        this.TotalAmmo = 0;
        this.AmmoInClip = 0;

        this.Arm = 0;
        this.AimOrigin = Vector3.Zero;
        this.AimDirection = Vector3.Zero;

        this.DamagerId = ElementId.Zero;
        this.DamageType = 0;
        this.DamageBodypart = 0;
    }
}
