using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using System;
using System.Collections.Generic;

namespace MtaServer.Server.PacketHandling
{
    public class PacketReducer
    {
        private readonly Dictionary<PacketId, List<IQueueHandler>> registeredQueueHandlers;

        public PacketReducer()
        {
            this.registeredQueueHandlers = new Dictionary<PacketId, List<IQueueHandler>>();
        }

        public void RegisterQueueHandler(PacketId packetId, IQueueHandler queueHandler)
        {
            if (!this.registeredQueueHandlers.ContainsKey(packetId))
            {
                this.registeredQueueHandlers[packetId] = new List<IQueueHandler>();
            }
            this.registeredQueueHandlers[packetId].Add(queueHandler);
        }

        public void EnqueuePacket(Client client, PacketId packetId, byte[] data)
        {
            if (this.registeredQueueHandlers.ContainsKey(packetId))
            {
                foreach (IQueueHandler queueHandler in this.registeredQueueHandlers[packetId])
                {
                    queueHandler.EnqueuePacket(client, packetId, data);
                }
            } else
            {
                Console.WriteLine($"Received unregistered packet {packetId}");
            }
        }
    }
}
