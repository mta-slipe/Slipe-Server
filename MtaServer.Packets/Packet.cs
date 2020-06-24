using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets
{
    public abstract class Packet
    {
        public abstract PacketId PacketId { get; }
        public abstract PacketFlags Flags { get; }

        public abstract byte[] Write();
        public abstract void Read(byte[] bytes);
    }
}
