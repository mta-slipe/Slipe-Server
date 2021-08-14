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
        public uint SourceElement { get; set; }
        public ushort? Latency { get; }


        public ProjectileSyncPacket()
        {

        }

        public ProjectileSyncPacket(Vector3 origin, Vector3 direction, uint sourceElement, byte weaponType, ushort model)
        {
            this.Model = model;
            this.SourceElement = sourceElement;
            this.WeaponType = weaponType;
            this.VecOrigin = origin;
            this.VecMoveSpeed = direction;
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
            }
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            if (this.SourceElement != 0)
            {
                builder.Write(true);
                builder.WriteElementId(this.SourceElement);

                builder.WriteCompressed(this.Latency ?? 0);
            }
            else
            {
                builder.Write(false);
            }

            if (this.OriginId != 0)
            {
                builder.Write(true);
                builder.WriteElementId(this.OriginId);
            }
            else
            {
                builder.Write(false);
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
                    builder.WriteVelocityVector(this.VecMoveSpeed);
                    break;
                case 19:            // WEAPONTYPE_ROCKET
                case 20:            // WEAPONTYPE_ROCKET_HS
                    if (this.TargetId != 0) // INVALID_ELEMENT_ID
                    {
                        builder.Write(true);
                        builder.Write(this.TargetId);
                    }
                    else
                    {
                        builder.Write(false);
                    }
                    builder.WriteVelocityVector(this.VecMoveSpeed);
                    builder.Write(this.VecRotation);
                    break;
            }

            return builder.Build();
        }
    }
}
