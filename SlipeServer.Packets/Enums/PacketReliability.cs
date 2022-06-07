namespace SlipeServer.Packets.Enums;

public enum PacketReliability
{
    Unreliable = 0,
    UnreliableSequenced,
    Reliable,

    // originally named ReliableOrdered
    ReliableSequenced,

    // originally named ReliableSequenced, Can drop packets, seems rarely used
    ReliableSequencedCanDropPackets
};
