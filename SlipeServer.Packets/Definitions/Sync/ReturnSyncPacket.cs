using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structures;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Definitions.Sync
{

    public class ReturnSyncPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_RETURN_SYNC;
        public override PacketReliability Reliability => PacketReliability.Reliable;
        public override PacketPriority Priority => PacketPriority.High;

        public Vector3 Position { get; set; }
        public float Rotation { get; set; }
        public bool IsInVechicle { get; set; }


        public ReturnSyncPacket()
        {

        }

        public ReturnSyncPacket(bool isInVehicle, Vector3 position, float rotation)
        {
            IsInVechicle = isInVehicle;
            Position = position;
            Rotation = rotation;
        }

        public ReturnSyncPacket(Vector3 position, float rotation)
        {
            IsInVechicle = true;
            Position = position;
            Rotation = rotation;
        }

        public ReturnSyncPacket(Vector3 position)
        {
            IsInVechicle = false;
            Position = position;
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            if (IsInVechicle)
            {
                builder.WriteVector3WithZAsFloat(this.Position);
                // TODO: write vehicle rotation
            } else
            {
                builder.WriteVector3WithZAsFloat(this.Position);
            }

            return builder.Build();
        }
    }
}
