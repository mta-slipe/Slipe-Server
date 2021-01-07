using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Player
{
    public class PlayerWastedPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_WASTED;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint PlayerId { get; set; }
        public bool IsStealth { get; set; }
        public byte TimeContext { get; set; }


        public ulong AnimationGroup { get; set; }
        public ulong AnimationId { get; set; }
        public uint KillerId { get; set; }
        public byte WeaponType { get; set; }
        public byte BodyPart { get; set; }
        public bool Stealth { get; }
        public Vector3 Position { get; set; }
        public ushort Ammo { get; set; }

        public PlayerWastedPacket()
        {

        }

        public PlayerWastedPacket(
            uint playerId,
            uint killerId,
            byte weaponType,
            byte bodyPart,
            bool stealth,
            byte timeContext,
            ulong animationGroup,
            ulong animationId
        )
        {
            this.PlayerId = playerId;
            this.KillerId = killerId;
            this.WeaponType = weaponType;
            this.BodyPart = bodyPart;
            this.Stealth = stealth;
            this.TimeContext = timeContext;
            this.AnimationGroup = animationGroup;
            this.AnimationId = animationId;
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.AnimationGroup = reader.GetCompressedUInt32();
            this.AnimationId = reader.GetCompressedUInt32();
            this.KillerId = reader.GetElementId();
            this.WeaponType = reader.GetByteCapped(6);
            this.BodyPart = reader.GetByteCapped(3);
            this.Position = reader.GetVector3WithZAsFloat();
            this.Ammo = reader.GetCompressedUint16();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write(this.PlayerId);
            builder.Write(this.KillerId);
            builder.WriteCapped(this.WeaponType, 6);
            builder.WriteCapped(this.BodyPart, 3);
            builder.Write(this.IsStealth);
            builder.Write(this.TimeContext);
            builder.WriteCompressed(this.AnimationGroup);
            builder.WriteCompressed(this.AnimationId);

            return builder.Build();
        }
    }
}
