using SlipeServer.Packets.Enums;
using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling;

public interface IQueueHandler
{
    IEnumerable<PacketId> SupportedPacketIds { get; }
    void EnqueuePacket(IClient client, PacketId packetId, byte[] data);
}
