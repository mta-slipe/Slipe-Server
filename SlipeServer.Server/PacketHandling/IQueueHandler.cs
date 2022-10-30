using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.PacketHandling;

public interface IQueueHandler
{
    IEnumerable<PacketId> SupportedPacketIds { get; }
    void EnqueuePacket(IClient client, PacketId packetId, byte[] data);
}
