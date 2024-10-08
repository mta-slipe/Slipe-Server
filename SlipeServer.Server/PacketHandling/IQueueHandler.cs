using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using System;
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
