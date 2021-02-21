using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Constants;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Vehicles
{
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
        public Vector3? AimOrigin { get; set; }
        public Vector3? AimDirection { get; set; }
        public VehicleAimDirection VehicleAimDirection { get; set; }
        public List<TrailerSync> Trailers { get; set; }
        public Vector2? TurretRotation { get; set; }
        public ushort? AdjustableProperty { get; set; }
        public ushort? LeftShoulder2 { get; set; }
        public ushort? RightShoulder2 { get; set; }

        public VehiclePureSyncPacket()
        {
            this.Trailers = new List<TrailerSync>();
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.TimeContext = reader.GetByte();
            this.KeySync = new FullKeySyncStructure();
            this.KeySync.Read(reader);
            this.RemoteModel = reader.GetInt32();
            this.Position = reader.GetVector3WithZAsFloat();

            this.CameraOrientation = new CameraOrientationStructure(this.Position);
            this.CameraOrientation.Read(reader);

            this.Seat = reader.GetVehicleSeat();

            bool isTrain = false;
            if (isTrain)
            {

            }

            if (this.Seat == 0)
            {
                this.Rotation = reader.GetVehicleRotation();
                this.Velocity = reader.GetVelocityVector();
                this.TurnVelocity = reader.GetVelocityVector();
                this.Health = reader.GetVehicleHealth();

                this.HasTrailer = reader.GetBit();
                if (this.HasTrailer)
                {
                    bool hasTrailer = true;
                    while (hasTrailer)
                    {
                        this.Trailers.Add(new TrailerSync()
                        {
                            Id = reader.GetElementId(),
                            Position = reader.GetVector3WithZAsFloat(),
                            Rotation = reader.GetVehicleRotation(),
                        });
                        hasTrailer = reader.GetBit();
                    }
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

                if (this.VehiclePureSyncFlags.IsDoingGangDriveby && WeaponConstants.weaponsWithAmmo.Contains(this.WeaponSlot.Value))
                {
                    this.WeaponAmmo = reader.GetAmmo();
                    this.WeaponAmmoInClip = reader.GetAmmo();

                    this.AimArm = ((reader.GetUint16()) * MathF.PI / 180) / 90.0f;
                    this.AimOrigin = reader.GetVector3();
                    this.AimDirection = reader.GetNormalizedVector();
                    this.VehicleAimDirection = (VehicleAimDirection)reader.GetByte();
                }
            }

            if (this.Seat == 0)
            {
                if (VehicleConstants.VehiclesWithTurrets.Contains(RemoteModel))
                {
                    this.TurretRotation = reader.GetTurretRotation();
                }

                if (VehicleConstants.VehiclesWithAdjustableProperties.Contains(RemoteModel))
                {
                    this.AdjustableProperty = reader.GetUint16();
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
            builder.Write(this.Latency);
            this.KeySync.Write(builder);
            builder.Write((int)this.RemoteModel);

            if (this.Seat == 0)
            {
                builder.WriteVector3WithZAsFloat(this.Position);
                bool isTrain = false;
                if (isTrain)
                {

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
                if (this.VehiclePureSyncFlags.IsDoingGangDriveby && WeaponConstants.weaponsWithAmmo.Contains(this.WeaponSlot ?? 0))
                {
                    builder.WriteAmmo(this.WeaponAmmo, this.WeaponAmmoInClip);

                    builder.Write((ushort)(this.AimArm * 90 * 180 * MathF.PI));
                    builder.Write(this.AimOrigin.Value);
                    builder.WriteNormalizedVector(this.AimDirection.Value);
                    builder.Write((byte)this.VehicleAimDirection);
                }
            }

            if (this.Seat == 0)
            {
                if (VehicleConstants.VehiclesWithTurrets.Contains(this.RemoteModel) && this.TurretRotation.HasValue)
                {
                    builder.WriteTurretRotation(this.TurretRotation.Value);
                }
                if (VehicleConstants.VehiclesWithAdjustableProperties.Contains(this.RemoteModel) && this.AdjustableProperty.HasValue)
                {
                    builder.Write(this.AdjustableProperty);
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
    }

    public struct TrailerSync
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
    }
}
