using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Sync
{
    public class WeaponBulletSyncPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_WEAPON_BULLETSYNC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint WeaponElementId { get; set; }
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
        public byte Counter { get; set; }

        public uint SourceElementId { get; set; }

        public WeaponBulletSyncPacket()
        {

        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.WeaponElementId = reader.GetElementId();
            this.Start = reader.GetVector3();
            this.End = reader.GetVector3();
            this.Counter = reader.GetByte();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.SourceElementId);
            builder.WriteElementId(this.WeaponElementId);
            builder.Write(this.Start);
            builder.Write(this.End);
            builder.Write(this.Counter);

            return builder.Build();
        }
    }
}
