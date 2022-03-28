using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Constants;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Vehicles;

public class VehiclePureSyncPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_VEHICLE_PURESYNC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.Medium;


    public uint PlayerId { get; set; }
    public ushort Latency { get; set; }
    public byte TimeContext { get; set; }
    public int RemoteModel { get; set; }
    public FullKeySyncStructure KeySync { get; set; } = new FullKeySyncStructure();
    public Vector3 Position { get; set; }

    public float TrainPosition { get; set; }
    public bool TrainDirection { get; set; }
    public byte TrainTrack { get; set; }
    public float TrainSpeed { get; set; }

    public float CameraRotation { get; set; }
    public CameraOrientationStructure CameraOrientation { get; set; } = new CameraOrientationStructure(Vector3.Zero);
    public byte Seat { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 TurnVelocity { get; set; }
    public float Health { get; set; }
    public bool HasTrailer { get; set; }
    public uint? DamagerId { get; set; }
    public byte? DamageWeaponType { get; set; }
    public byte? DamageBodyPart { get; set; }
    public float PlayerHealth { get; set; }
    public float PlayerArmor { get; set; }
    public VehiclePureSyncFlagsStructure VehiclePureSyncFlags { get; set; } = new VehiclePureSyncFlagsStructure();
    public byte? WeaponSlot { get; set; }
    public ushort? WeaponAmmo { get; set; }
    public ushort? WeaponAmmoInClip { get; set; }
    public float? AimArm { get; set; }
    public Vector3 AimOrigin { get; set; }
    public Vector3 AimDirection { get; set; }
    public VehicleAimDirection VehicleAimDirection { get; set; }
    public List<TrailerSync> Trailers { get; set; }
    public Vector2 TurretRotation { get; set; }
    public ushort AdjustableProperty { get; set; }
    public float[] DoorOpenRatios { get; set; }
    public ushort LeftShoulder2 { get; set; }
    public ushort RightShoulder2 { get; set; }

    public VehiclePureSyncPacket()
    {
        this.Trailers = new List<TrailerSync>();
        this.DoorOpenRatios = new float[6];
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        this.TimeContext = reader.GetByte();
        this.KeySync = new FullKeySyncStructure();
        this.KeySync.Read(reader);
        this.RemoteModel = reader.GetInt32();
        this.Position = reader.GetVector3WithZAsFloat();

        bool isTrain = VehicleConstants.Trains.Contains(this.RemoteModel);
        if (isTrain)
        {
            this.TrainPosition = reader.GetFloat();
            this.TrainDirection = reader.GetBit();
            this.TrainTrack = reader.GetByte();
            this.TrainSpeed = reader.GetFloat();
        }

        this.CameraOrientation = new CameraOrientationStructure(this.Position);
        this.CameraOrientation.Read(reader);

        this.Seat = reader.GetVehicleSeat();

        if (this.Seat == 0)
        {
            this.Rotation = reader.GetVehicleRotation();
            this.Velocity = reader.GetVelocityVector();
            this.TurnVelocity = reader.GetVelocityVector();
            this.Health = reader.GetVehicleHealth();

            while (reader.GetBit())
            {
                this.Trailers.Add(new TrailerSync()
                {
                    Id = reader.GetElementId(),
                    Position = reader.GetVector3WithZAsFloat(),
                    Rotation = reader.GetVehicleRotation(),
                });
            }
        }

        var damaged = reader.GetBit();
        if (damaged)
        {
            this.DamagerId = reader.GetElementId();
            this.DamageWeaponType = reader.GetWeaponType();
            this.DamageBodyPart = reader.GetBodyPart();
        }

        this.PlayerHealth = reader.GetPlayerHealth();
        this.PlayerArmor = reader.GetPlayerArmor();

        this.VehiclePureSyncFlags.Read(reader);

        if (this.VehiclePureSyncFlags.HasAWeapon)
        {
            this.WeaponSlot = reader.GetWeaponSlot();

            if (this.VehiclePureSyncFlags.IsDoingGangDriveby && WeaponConstants.WeaponsWithAmmo.Contains(this.WeaponSlot.Value))
            {
                this.WeaponAmmo = reader.GetAmmo();
                this.WeaponAmmoInClip = reader.GetAmmo();

                this.AimArm = ((reader.GetInt16()) * MathF.PI / 180) / 90.0f;
                this.AimOrigin = reader.GetVector3();
                this.AimDirection = reader.GetNormalizedVector();
                this.VehicleAimDirection = (VehicleAimDirection)reader.GetByteCapped(2);
            }
        }

        if (this.Seat == 0)
        {
            if (VehicleConstants.VehiclesWithTurrets.Contains(this.RemoteModel))
            {
                this.TurretRotation = reader.GetTurretRotation();
            }

            if (VehicleConstants.VehiclesWithAdjustableProperties.Contains(this.RemoteModel))
            {
                this.AdjustableProperty = reader.GetUint16();
            }

            if (VehicleConstants.VehiclesWithDoors.Contains(this.RemoteModel))
            {
                this.DoorOpenRatios = new float[6];
                for (int i = 2; i < 6; i++)
                {
                    bool isNotCompressed = reader.GetBit();
                    if (!isNotCompressed)
                    {
                        if (reader.GetBit())
                            this.DoorOpenRatios[i] = 1.0f;
                        else
                            this.DoorOpenRatios[i] = 0.0f;
                    } else
                    {
                        this.DoorOpenRatios[i] = reader.GetFloatFromBits(10, 0.0f, 1.0f);
                    }
                }
            }
        }

        if (this.VehiclePureSyncFlags.IsAircraft)
        {
            this.LeftShoulder2 = reader.GetBit() ? (ushort)255 : (ushort)0;
            this.RightShoulder2 = reader.GetBit() ? (ushort)255 : (ushort)0;
        }
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.WriteElementId(this.PlayerId);
        builder.Write(this.TimeContext);
        builder.WriteCompressed(this.Latency);
        this.KeySync.Write(builder);
        builder.Write((int)this.RemoteModel);

        if (this.Seat == 0)
        {
            builder.WriteVector3WithZAsFloat(this.Position);
            bool isTrain = false;
            if (isTrain)
            {
                builder.Write(this.TrainPosition);
                builder.Write(this.TrainDirection);
                builder.Write(this.TrainTrack);
                builder.Write(this.TrainSpeed);
            }

            builder.WriteVehicleRotation(this.Rotation);
            builder.WriteVelocityVector(this.Velocity);
            builder.WriteVelocityVector(this.TurnVelocity);
            builder.WriteVehicleHealth(this.Health);

            var trailers = new Queue<TrailerSync>(this.Trailers);
            while (trailers.Any())
            {
                builder.Write(true);
                var trailer = trailers.Dequeue();
                builder.WriteElementId(trailer.Id);
                builder.WriteVector3WithZAsFloat(trailer.Position);
                builder.WriteVehicleRotation(trailer.Rotation);
            }
            builder.Write(false);
        }

        builder.WritePlayerHealth(this.PlayerHealth);
        builder.WritePlayerArmor(this.PlayerArmor);

        this.VehiclePureSyncFlags.Write(builder);

        if (this.VehiclePureSyncFlags.HasAWeapon)
        {
            builder.WriteWeaponSlot(this.WeaponSlot ?? 0);
            if (this.VehiclePureSyncFlags.IsDoingGangDriveby && WeaponConstants.WeaponsWithAmmo.Contains(this.WeaponSlot ?? 0))
            {
                builder.WriteAmmo(this.WeaponAmmo, this.WeaponAmmoInClip);

                builder.Write((ushort)(this.AimArm! * 90 * 180 / MathF.PI));
                builder.Write(this.AimOrigin);
                builder.WriteNormalizedVector(this.AimDirection);
                builder.WriteCapped((byte)this.VehicleAimDirection, 2);
            }
        }

        if (this.Seat == 0)
        {
            if (VehicleConstants.VehiclesWithTurrets.Contains(this.RemoteModel))
            {
                builder.WriteTurretRotation(this.TurretRotation);
            }
            if (VehicleConstants.VehiclesWithAdjustableProperties.Contains(this.RemoteModel))
            {
                builder.Write(this.AdjustableProperty);
            }

            if (VehicleConstants.VehiclesWithDoors.Contains(this.RemoteModel) && this.DoorOpenRatios != null)
            {
                for (int i = 2; i < 6; i++)
                {
                    var ratio = this.DoorOpenRatios[i];
                    if (ratio == 0 || ratio == 1)
                    {
                        builder.Write(false);
                        builder.Write(ratio == 1);
                    } else
                    {
                        builder.Write(true);
                        builder.WriteFloatFromBits(ratio, 12, 0.0f, 1.0f, true);
                    }
                }
            }
        }

        if (this.VehiclePureSyncFlags.IsAircraft)
        {
            builder.Write(this.LeftShoulder2 != 0);
            builder.Write(this.RightShoulder2 != 0);
        }

        builder.WriteCapped(0, 4);

        return builder.Build();
    }

    public override void Reset()
    {
        this.TrainPosition = default;
        this.TrainDirection = default;
        this.TrainTrack = default;
        this.TrainSpeed = default;

        this.Rotation = default;
        this.Velocity = default;
        this.TurnVelocity = default;
        this.Health = default;

        this.Trailers.Clear();

        this.DamagerId = default;
        this.DamageWeaponType = default;
        this.DamageBodyPart = default;

        this.PlayerHealth = default;
        this.PlayerArmor = default;

        this.VehiclePureSyncFlags = new();

        this.WeaponSlot = default;

        this.WeaponAmmo = default;
        this.WeaponAmmoInClip = default;

        this.AimArm = default;
        this.AimOrigin = default;
        this.AimDirection = default;
        this.VehicleAimDirection = default;


        this.TurretRotation = default;
        this.AdjustableProperty = default;

        this.DoorOpenRatios = new float[6];

        this.LeftShoulder2 = default;
        this.RightShoulder2 = default;
    }
}

public struct TrailerSync
{
    public uint Id { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
}
