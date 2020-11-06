using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.PacketHandling
{
    public interface IQueueHandler
    {
        void EnqueuePacket(Client client, PacketId packetId, byte[] data);
    }
}
