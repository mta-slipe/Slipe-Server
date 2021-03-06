using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Packets
{
    public abstract class Packet
    {
        public abstract PacketId PacketId { get; }
        public abstract PacketReliability Reliability { get; }
        public abstract PacketPriority Priority { get; }

        public abstract byte[] Write();
        public abstract void Read(byte[] bytes);
    }
}
