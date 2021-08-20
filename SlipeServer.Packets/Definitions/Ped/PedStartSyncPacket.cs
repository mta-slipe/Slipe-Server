using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Ped
{
    public class PedStartSyncPacket : Packet
    {
        public override PacketId PacketId { get; } = PacketId.PACKET_ID_PED_STARTSYNC;
        public override PacketReliability Reliability { get; } = PacketReliability.ReliableSequenced;
        public override PacketPriority Priority { get; } = PacketPriority.High;

        public uint SourceElementId { get; set; }
        public Vector3 Position { get; set; }
        public float Rotation { get; set; }
        public Vector3 VelocityVector { get; set; }
        public float Health { get; set; }
        public float Armor { get; set; }

        public PedStartSyncPacket(uint sourceElementId, Vector3 position, float rotation, Vector3 velocityVector, float health, float armor)
        {
            SourceElementId = sourceElementId;
            Position = position;
            Rotation = rotation;
            VelocityVector = velocityVector;
            Health = health;
            Armor = armor;
        }

        public PedStartSyncPacket(uint sourceElementId)
        {
            this.SourceElementId = sourceElementId;
        }

        public PedStartSyncPacket()
        {
            
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(SourceElementId);

            builder.Write(Position.X);
            builder.Write(Position.Y);
            builder.Write(Position.Z);

            builder.Write(Rotation);

            builder.Write(VelocityVector.X);
            builder.Write(VelocityVector.Y);
            builder.Write(VelocityVector.Z);

            builder.Write(Health);
            builder.Write(Armor);

            return builder.Build();
        }

        public override void Read(byte[] bytes)
        {
            
        }
    }
}
