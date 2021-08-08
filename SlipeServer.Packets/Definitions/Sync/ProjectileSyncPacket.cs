using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structures;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Constants;

namespace SlipeServer.Packets.Definitions.Sync
{
    public class ProjectileSyncPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PROJECTILE;
        public override PacketReliability Reliability => PacketReliability.Reliable;
        public override PacketPriority Priority => PacketPriority.Medium;

        public byte WeaponType { get; set; }
        public uint OriginId { get; set; }
        public Vector3 VecOrigin { get; set; }
        public float Force { get; set; }
        public byte HasTarget { get; set; }
        public uint TargetId { get; set; }
        public Vector3 VecTarget { get; set; }
        public Vector3 VecRotation { get; set; }
        public Vector3 VecMoveSpeed { get; set; }
        public ushort Model { get; set; }


        public ProjectileSyncPacket()
        {

        }

        public ProjectileSyncPacket(SmallKeySyncStructure smallKeySyncStructure, float playerRotation, float cameraRotation, KeySyncFlagsStructure keySyncFlagsStructure)
        {

        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            bool hasOrigin = reader.GetBit();
            if (hasOrigin)
                this.OriginId = reader.GetUint32();

            this.VecOrigin = reader.GetVector3WithZAsFloat();
            this.WeaponType = reader.GetByte();
            this.Model = reader.GetUint16();

            switch(WeaponType)
            {
                case 16: // WEAPONTYPE_GRENADE
                case 17: // WEAPONTYPE_TEARGAS
                case 18: // WEAPONTYPE_MOLOTOV
                case 39: // WEAPONTYPE_REMOTE_SATCHEL_CHARGE
                    this.Force = reader.GetFloatFromBits(24, -128, 128); // 7 integer bits, 2^7 = 128
                    this.VecMoveSpeed = reader.GetVelocityVector();
                    break;
                case 19: // WEAPONTYPE_ROCKET
                case 20: // WEAPONTYPE_ROCKET_HS
                    bool hasTarget = reader.GetBit();
                    if (hasTarget)
                        this.TargetId = reader.GetUint32();

                    this.VecMoveSpeed = reader.GetVelocityVector();
                    this.VecRotation = reader.GetVector3();

                    break;
                //case 58:            // WEAPONTYPE_FLARE
                //case 21:            // WEAPONTYPE_FREEFALL_BOMB
                //    break;

                //default:
                //    break;
            }
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.PlayerId);

            this.SmallKeySyncStructure.Write(builder);

            builder.WriteFloatFromBits(this.PlayerRotation, 12, -MathF.PI, MathF.PI, true);
            builder.WriteFloatFromBits(this.CameraRotation, 12, -MathF.PI, MathF.PI, true);

            this.KeySyncFlagsStructure.Write(builder);

            if (this.SmallKeySyncStructure.ButtonCircle || this.SmallKeySyncStructure.RightShoulder1)
            {
                builder.WriteCapped(this.WeaponSlot, 4);

                if (WeaponConstants.weaponsWithAmmo.Contains(this.WeaponSlot))
                {
                    builder.WriteCompressed(this.TotalAmmo);
                    builder.WriteCompressed(this.AmmoInClip);

                    builder.Write((ushort)(this.AimArm * 90 * 180 * MathF.PI));
                    builder.Write(this.AimOrigin);
                    builder.WriteNormalizedVector(this.AimDirection);
                    builder.Write((byte)this.VehicleAimDirection);
                }
            }

            if (this.KeySyncFlagsStructure.IsSyncingVehicle)
            {
                // write turret if vehicle has turret
                // write hydraulics if upgrade present
                // write should buttons if in plane or heli
            }

            return builder.Build();
        }
    }
}
