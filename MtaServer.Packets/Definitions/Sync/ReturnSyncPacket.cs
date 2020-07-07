using MtaServer.Packets.Enums;
using MtaServer.Packets.Structures;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using MtaServer.Packets.Builder;
using MtaServer.Packets.Reader;

namespace MtaServer.Packets.Definitions.Sync
{

    public class ReturnSyncPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_RETURN_SYNC;
        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

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
