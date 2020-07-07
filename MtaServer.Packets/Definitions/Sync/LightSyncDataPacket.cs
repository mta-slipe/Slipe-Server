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

    public class LightSyncDataPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LIGHTSYNC;
        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;



        public LightSyncDataPacket()
        {

        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            return builder.Build();
        }
    }
}
