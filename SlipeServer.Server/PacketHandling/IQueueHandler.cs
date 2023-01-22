using SlipeServer.Packets.Enums;
using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling;

/// <summary>
/// Interface that allows you to enqueue packets to be handled
/// </summary>
public interface IQueueHandler
{
    IEnumerable<PacketId> SupportedPacketIds { get; }
    void EnqueuePacket(IClient client, PacketId packetId, byte[] data);
}
