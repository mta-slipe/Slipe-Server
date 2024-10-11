using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.TestTools;

public struct SendPacketCall
{
    public ulong Address { get; set; }
    public ushort Version { get; set; }

    public PacketId PacketId { get; set; }
    public byte[] Data { get; set; }
    public PacketReliability Reliability { get; set; }
    public PacketPriority Priority { get; set; }
}
