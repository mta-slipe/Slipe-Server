using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using MtaServer.Packets.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtaServer.Packets.Definitions.Resources
{
    public class ResourceStopPacket : Packet
    {

        public override PacketId PacketId => PacketId.PACKET_ID_RESOURCE_STOP;
        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public ushort NetId { get; }

        public ResourceStopPacket(
            ushort netId
        )
        {
            NetId = netId;
        }

        public override void Read(byte[] bytes)
        {
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write(NetId);

            return builder.Build();
        }
    }
}
