using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using MtaServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Player
{
    public class SpawnPlayerPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_SPAWN;
        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

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
            PlayerId = playerId;
            Position = position;
            SpawnFlags = flags;
            Rotation = rotation;
            Skin = skin;
            TeamId = teamId;
            Interior = interior;
            Dimension = dimension;
            TimeContext = timeContext;
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
            builder.Write(this.TeamId);
            builder.Write(this.TimeContext);

            return builder.Build();
        }
    }
}
