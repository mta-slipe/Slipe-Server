using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.Clients;

public struct QueuedClientPacket
{
    public PacketId PacketId { get; set; }
    public byte[] Data { get; set; }
    public PacketPriority priority { get; set; }
    public PacketReliability reliability { get; set; }
}
