using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Player
{
    public class SpawnPlayerPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_SPAWN;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint PlayerId { get; set; }
        public Vector3 Position { get; set; }
        public byte SpawnFlags { get; set; }
        public float Rotation { get; set; }
        public ushort Skin { get; set; }
        public uint TeamId { get; set; }
        public byte Interior { get; set; }
        public ushort Dimension { get; set; }
        public byte TimeContext { get; set; }

        public SpawnPlayerPacket()
        {

        }

        public SpawnPlayerPacket(uint playerId, byte flags, Vector3 position, float rotation, ushort skin, uint teamId, byte interior, ushort dimension, byte timeContext)
        {
            this.PlayerId = playerId;
            this.Position = position;
            this.SpawnFlags = flags;
            this.Rotation = rotation;
            this.Skin = skin;
            this.TeamId = teamId;
            this.Interior = interior;
            this.Dimension = dimension;
            this.TimeContext = timeContext;
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.PlayerId = reader.GetElementId();

            this.SpawnFlags = reader.GetByte();

            this.Position = reader.GetVector3();
            this.Rotation = reader.GetFloat();
            this.Skin = reader.GetUint16();
            this.Interior = reader.GetByte();
            this.Dimension = reader.GetUint16();
            this.TeamId = reader.GetElementId();
            this.TimeContext = reader.GetByte();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.PlayerId);

            builder.Write(this.SpawnFlags);

            builder.Write(this.Position);
            builder.Write(this.Rotation);
            builder.Write(this.Skin);
            builder.Write(this.Interior);
            builder.Write(this.Dimension);
            builder.WriteElementId(this.TeamId);
            builder.Write(this.TimeContext);

            return builder.Build();
        }
    }
}
