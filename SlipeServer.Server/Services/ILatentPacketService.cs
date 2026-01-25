using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using System.Collections.Generic;

namespace SlipeServer.Server.Services;

public interface ILatentPacketService
{
    void EnqueueLatentPacket(IEnumerable<Player> players, Packet packet, ushort resourceNetId, int rate = 50000);
    void EnqueueLatentPacket(IEnumerable<Player> players, PacketId packetId, byte[] data, ushort resourceNetId, int rate = 50000, PacketPriority priority = PacketPriority.Medium, PacketReliability reliability = PacketReliability.Unreliable);
}