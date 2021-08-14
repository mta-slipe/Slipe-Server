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

    public class LightSyncDataPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LIGHTSYNC;
        public override PacketReliability Reliability => PacketReliability.Unreliable;
        public override PacketPriority Priority => PacketPriority.Low;



        public LightSyncDataPacket()
        {

        }

        public override void Read(byte[] bytes)
        {
            //var reader = new PacketReader(bytes);
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            return builder.Build();
        }
    }
}
