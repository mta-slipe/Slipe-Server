using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Constants;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;
using SlipeServer.Packets.Structures;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Sync;

public sealed class KeySyncPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_KEYSYNC;
    public override PacketReliability Reliability => PacketReliability.UnreliableSequenced;
    public override PacketPriority Priority => PacketPriority.Medium;

    public SmallKeySyncStructure SmallKeySyncStructure { get; set; } = new SmallKeySyncStructure();
    public float PlayerRotation { get; set; }
    public float CameraRotation { get; set; }
    public KeySyncFlagsStructure KeySyncFlagsStructure { get; set; } = new KeySyncFlagsStructure();


    public bool HasWeapon { get; set; }
    public ushort AmmoInClip { get; set; }
    public byte WeaponType { get; set; }
    public byte WeaponSlot { get; set; }

    public Vector3 AimOrigin { get; set; }
    public Vector3 AimDirection { get; set; }
    public float AimArm { get; set; }
    public VehicleAimDirection VehicleAimDirection { get; set; }

    public Vector2? TurretRotation { get; set; }

    public short? HydraulicsRightStickX { get; set; }
    public short? HydraulicsRightStickY { get; set; }

    public short? HydraulicsOrTurretRotationX { get; set; }
    public short? HydraulicsOrTurretRotationY { get; set; }

    public bool? PlaneLeftShoulder2 { get; set; }
    public bool? PlaneRightShoulder2 { get; set; }

    public ElementId PlayerId { get; set; }

    public KeySyncPacket()
    {

    }

    public KeySyncPacket(SmallKeySyncStructure smallKeySyncStructure, float playerRotation, float cameraRotation, KeySyncFlagsStructure keySyncFlagsStructure)
    {
        this.SmallKeySyncStructure = smallKeySyncStructure;
        this.PlayerRotation = playerRotation;
        this.CameraRotation = cameraRotation;
        this.KeySyncFlagsStructure = keySyncFlagsStructure;
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        this.SmallKeySyncStructure = new SmallKeySyncStructure();
        this.SmallKeySyncStructure.Read(reader);

        this.PlayerRotation = reader.GetFloatFromBits(12, -MathF.PI, MathF.PI);
        this.CameraRotation = reader.GetFloatFromBits(12, -MathF.PI, MathF.PI);

        this.KeySyncFlagsStructure = new KeySyncFlagsStructure();
        this.KeySyncFlagsStructure.Read(reader);

        if (this.SmallKeySyncStructure.ButtonCircle || this.SmallKeySyncStructure.RightShoulder1)
        {
            this.HasWeapon = reader.GetBit();
            if (this.HasWeapon)
            {
                this.WeaponType = reader.GetByte();
                this.WeaponSlot = reader.GetWeaponSlot();

                if (WeaponConstants.WeaponsWithAmmo.Contains(this.WeaponSlot))
                {
                    this.AmmoInClip = reader.GetAmmo();

                    this.AimArm = ((reader.GetInt16()) * MathF.PI / 180f) / 90.0f;
                    this.AimOrigin = reader.GetVector3();
                    this.AimDirection = reader.GetNormalizedVector();
                    this.VehicleAimDirection = (VehicleAimDirection)reader.GetByteCapped(2);
                }
            }
        }

        ReadVehicleSpecific(reader);
    }

    // MTA fetches the vehicle and uses that to determine whether or not data should be read
    // We however do not want to bring elements into packet definitions so instead we make
    // some assumptions based on the size of the packet
    private void ReadVehicleSpecific(PacketReader reader)
    {
        if (this.KeySyncFlagsStructure.IsSyncingVehicle)
        {
            var dataLeft = reader.Size - reader.Counter;

            // the packet contains both the turret rotation AND hydraulics control state
            // if it only contains one of the two we can not determine which it is, so we 
            if (dataLeft >= 64)
            {
                this.TurretRotation = reader.GetTurretRotation();
                this.HydraulicsRightStickX = reader.GetInt16();
                this.HydraulicsRightStickY = reader.GetInt16();

                dataLeft -= 64;
            } else if (dataLeft >= 32)
            {
                this.HydraulicsOrTurretRotationX = reader.GetInt16();
                this.HydraulicsOrTurretRotationY = reader.GetInt16();

                dataLeft -= 32;
            }

            if (dataLeft >= 2)
            {
                this.PlaneLeftShoulder2 = reader.GetBit();
                this.PlaneRightShoulder2 = reader.GetBit();
            }
        }
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.PlayerId);

        this.SmallKeySyncStructure.Write(builder);

        builder.WriteFloatFromBits(this.PlayerRotation, 12, -MathF.PI, MathF.PI, true);
        builder.WriteFloatFromBits(this.CameraRotation, 12, -MathF.PI, MathF.PI, true);

        this.KeySyncFlagsStructure.Write(builder);

        if (this.SmallKeySyncStructure.ButtonCircle || this.SmallKeySyncStructure.RightShoulder1)
        {
            builder.WriteWeaponSlot(this.WeaponSlot);

            if (WeaponConstants.WeaponsWithAmmo.Contains(this.WeaponSlot))
            {
                builder.WriteAmmo(this.AmmoInClip);

                builder.Write((short)(this.AimArm * 90f * 180f / MathF.PI));
                builder.Write(this.AimOrigin);
                builder.WriteNormalizedVector(this.AimDirection);
                builder.Write((byte)this.VehicleAimDirection);
            }
        }

        if (this.KeySyncFlagsStructure.IsSyncingVehicle)
        {
            if (this.TurretRotation != null)            
                builder.WriteTurretRotation(this.TurretRotation.Value);

            if (this.HydraulicsRightStickX.HasValue && this.HydraulicsRightStickY.HasValue)
            {
                builder.Write(this.HydraulicsRightStickX.Value);
                builder.Write(this.HydraulicsRightStickY.Value);
            }

            if (this.HydraulicsOrTurretRotationX.HasValue && this.HydraulicsOrTurretRotationY.HasValue)
            {
                builder.Write(this.HydraulicsOrTurretRotationX.Value);
                builder.Write(this.HydraulicsOrTurretRotationY.Value);
            }

            if (this.PlaneLeftShoulder2.HasValue && this.PlaneRightShoulder2.HasValue)
            {
                builder.Write(this.PlaneLeftShoulder2);
                builder.Write(this.PlaneRightShoulder2);
            }
        }

        return builder.Build();
    }

    public override void Reset()
    {
        this.HasWeapon = false;
        this.WeaponType = 0;
        this.WeaponSlot = 0;
        this.AmmoInClip = 0;

        this.AimArm = 0;
        this.AimOrigin = Vector3.Zero;
        this.AimDirection = Vector3.Zero;
        this.VehicleAimDirection = 0;
    }
}
