using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Packets.Enums
{
    [Flags]
    public enum PacketFlags
    {
        PACKET_RELIABLE = 1,
        PACKET_SEQUENCED = 2,
        PACKET_HIGH_PRIORITY = 4,
        PACKET_LOW_PRIORITY = 8,

        PACKET_UNRELIABLE = 0,
        PACKET_MEDIUM_PRIORITY = 0,
    }

    public enum NetServerPacketPriority
    {
        PACKET_PRIORITY_HIGH = 0,
        PACKET_PRIORITY_MEDIUM,
        PACKET_PRIORITY_LOW,
        PACKET_PRIORITY_COUNT
    };

    public enum NetServerPacketReliability
    {
        PACKET_RELIABILITY_UNRELIABLE = 0,
        PACKET_RELIABILITY_UNRELIABLE_SEQUENCED,
        PACKET_RELIABILITY_RELIABLE,
        PACKET_RELIABILITY_RELIABLE_ORDERED,
        PACKET_RELIABILITY_RELIABLE_SEQUENCED            //     Can drop packets
    };
}
