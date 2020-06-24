using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.PacketHandling
{
    public interface IQueueHandler
    {
        void EnqueuePacket(Client client, PacketId packetId, byte[] data);
    }
}
