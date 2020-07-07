using MtaServer.Packets.Enums;
using MtaServer.Packets.Structures;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using MtaServer.Packets.Builder;
using MtaServer.Packets.Reader;

namespace MtaServer.Packets.Definitions.Sync
{

    public class PlayerPureSyncPacket : Packet
    {
        static readonly HashSet<byte> slotsWithAmmo = new HashSet<byte>()
        {
            2, 3, 4, 5, 6, 7, 8, 9
        };

        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_PURESYNC;
        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public uint PlayerId { get; set; }
        public byte TimeContext { get; set; }
        public ushort Latency { get; set; }
        public FullKeySyncStructure KeySync { get; set; } = new FullKeySyncStructure();
        public PlayerPureSyncFlagsStructure SyncFlags { get; set; } = new PlayerPureSyncFlagsStructure();
        public uint ContactElementId { get; set; }
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
        public uint DamagerId { get; set; }
        public byte DamageBodypart { get; set; }
        public byte DamageBodyPart { get; set; }

        public PlayerPureSyncPacket()
        {

        }

        public PlayerPureSyncPacket(byte timeContext)
        {
            TimeContext = timeContext;
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

                if (slotsWithAmmo.Contains(this.WeaponSlot))
                {
                    this.TotalAmmo = reader.GetCompressedUint16();
                    this.AmmoInClip = reader.GetCompressedUint16();

                    this.Arm = ((reader.GetUint16()) * MathF.PI / 180) / 90.0f;

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
                this.WeaponType = reader.GetByteCapped(6);
                this.DamageBodypart = reader.GetByteCapped(3);
            }
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.PlayerId);
            builder.Write(this.TimeContext);
            builder.WriteCompressed(this.Latency);

            this.KeySync.Write(builder);
            this.SyncFlags.Write(builder);
            if (this.SyncFlags.HasContact)
            {
                builder.WriteElementId(this.ContactElementId);
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
                builder.Write(this.WeaponType);
                builder.WriteCapped(this.WeaponSlot, 4);

                if (slotsWithAmmo.Contains(this.WeaponSlot))
                {
                    builder.WriteCompressed(this.TotalAmmo);
                    builder.WriteCompressed(this.AmmoInClip);

                    builder.Write(this.Arm * 90 * 180 * MathF.PI);
                    //this.Arm = ((reader.GetUint16()) * MathF.PI / 180) / 90.0f;

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
    }
}
