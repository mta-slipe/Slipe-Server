using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Sync
{
    public class PlayerBulletSyncPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_BULLETSYNC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public byte WeaponType { get; set; }
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
        public byte Counter { get; set; }

        public float? Damage { get; set; }
        public byte? BodyPart { get; set; }
        public uint? DamagedElementId { get; set; }

        public uint SourceElementId { get; set; }

        public PlayerBulletSyncPacket()
        {

        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.WeaponType = reader.GetByte();
            this.Start = reader.GetVector3();
            this.End = reader.GetVector3();
            this.Counter = reader.GetByte();

            if (reader.GetBit())
            {
                this.Damage = reader.GetFloat();
                this.BodyPart = reader.GetByte();
                this.DamagedElementId = reader.GetElementId();
            }
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.SourceElementId);
            builder.Write(this.WeaponType);
            builder.Write(this.Start);
            builder.Write(this.End);
            builder.Write(this.Counter);

            var hasDamaged = this.Damage.HasValue && this.BodyPart.HasValue && this.DamagedElementId.HasValue;
            builder.Write(hasDamaged);
            if (hasDamaged)
            {
                builder.Write(this.Damage!.Value);
                builder.Write(this.BodyPart!.Value);
                builder.WriteElementId(this.DamagedElementId!.Value);
            }

            return builder.Build();
        }
    }
}
