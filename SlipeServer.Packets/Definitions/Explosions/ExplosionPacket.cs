using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Explosions
{
    public class ExplosionPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_EXPLOSION;

        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

        public override PacketPriority Priority => PacketPriority.High;

        public uint? PlayerSource { get; set; }
        public uint? OriginId { get; set; }
        public Vector3 Position { get; set; }
        public byte ExplosionType { get; set; }
        public ushort? Latency { get; set; }

        public ExplosionPacket()
        {

        }

        public ExplosionPacket(uint? playerSource, uint? originId, Vector3 position, byte explosionType, ushort latency)
        {
            this.PlayerSource = playerSource;
            this.OriginId = originId;
            this.Position = position;
            this.ExplosionType = explosionType;
            this.Latency = latency;
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            if (reader.GetBit())
            {
                this.OriginId = reader.GetElementId();
            }
            this.Position = reader.GetVector3WithZAsFloat();
            this.ExplosionType = reader.GetByteCapped(4);
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write(this.PlayerSource.HasValue);
            if (this.PlayerSource.HasValue)
            {
                builder.WriteElementId(this.PlayerSource.Value);
                builder.WriteCompressed(this.Latency ?? 0);
            }

            builder.Write(this.OriginId.HasValue);
            if (this.OriginId.HasValue)
            {
                builder.WriteElementId(this.OriginId.Value);
            }

            builder.WriteVector3WithZAsFloat(this.Position);
            builder.WriteCapped(this.ExplosionType, 4);

            return builder.Build();
        }

        public override void Reset()
        {
            this.OriginId = null;
            this.PlayerSource = null;
            this.Latency = null;
        }
    }
}
