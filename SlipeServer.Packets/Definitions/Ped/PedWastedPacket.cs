using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Constants;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Definitions.Ped
{
    public class PedWastedPacket : Packet
    {
        public override PacketId PacketId { get; } = PacketId.PACKET_ID_PED_WASTED;
        public override PacketReliability Reliability { get; } = PacketReliability.ReliableSequenced;
        public override PacketPriority Priority { get; } = PacketPriority.High;

        public uint PedId { get; set; }
        public uint KillerId { get; set; }
        public byte KillerWeapon { get; set; }
        public byte BodyPart { get; set; }
        public Vector3 Position { get; set; }
        public ushort Ammo { get; set; }
        public bool Stealth { get; set; }
        public byte TimeContext { get; set; }
        public ulong AnimGroup { get; set; }
        public ulong AnimId { get; set; }

        public PedWastedPacket(uint pedId, uint killerId, byte killerWeapon, byte bodyPart, Vector3 position, ushort ammo, bool stealth, byte timeContext, ulong animGroup, ulong animId)
        {
            this.PedId = pedId;
            this.KillerId = killerId;
            this.KillerWeapon = killerWeapon;
            this.BodyPart = bodyPart;
            this.Position = position;
            this.Ammo = ammo;
            this.Stealth = stealth;
            this.TimeContext = timeContext;
            this.AnimGroup = animGroup;
            this.AnimId = animId;
        }

        public PedWastedPacket()
        {
            
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.PedId);
            builder.WriteElementId(this.KillerId);

            builder.Write(this.KillerWeapon);

            builder.Write(this.BodyPart);

            builder.Write(this.Stealth);

            builder.Write(this.TimeContext);

            builder.WriteCompressed(this.AnimGroup);
            builder.WriteCompressed(this.AnimId);

            return builder.Build();
        }

        public override void Read(byte[] bytes)
        {
            var data = new PacketReader(bytes);

            this.AnimGroup = data.GetCompressedByte();
            this.AnimId = data.GetCompressedByte();
            this.KillerId = data.GetElementId();
            this.KillerWeapon = data.GetByte();
            this.BodyPart = data.GetByte();
            this.Position = data.GetVector3WithZAsFloat();
            this.PedId = data.GetElementId();

            this.Ammo = data.GetByte();
        }
    }
}
