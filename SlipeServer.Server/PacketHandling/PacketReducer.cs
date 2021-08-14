using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SlipeServer.Server.PacketHandling
{
    public class PacketReducer
    {
        private readonly List<IQueueHandler> queueHandlers;
        private readonly Dictionary<PacketId, List<IQueueHandler>> registeredQueueHandlers;
        private readonly ILogger logger;

        public IEnumerable<IQueueHandler> RegisteredQueueHandlers => this.queueHandlers;

        public PacketReducer(ILogger logger)
        {
            this.queueHandlers = new List<IQueueHandler>();
            this.registeredQueueHandlers = new Dictionary<PacketId, List<IQueueHandler>>();
            this.logger = logger;
        }

        public void RegisterQueueHandler(PacketId packetId, IQueueHandler queueHandler)
        {
            if (!this.registeredQueueHandlers.ContainsKey(packetId))
            {
                this.registeredQueueHandlers[packetId] = new List<IQueueHandler>();
            }
            this.registeredQueueHandlers[packetId].Add(queueHandler);
            this.queueHandlers.Add(queueHandler);
        }

        public void UnregisterQueueHandler(PacketId packetId, IQueueHandler queueHandler)
        {
            if (this.registeredQueueHandlers.ContainsKey(packetId))
            {
                this.registeredQueueHandlers[packetId].Remove(queueHandler);
            }
            this.queueHandlers.Add(queueHandler);
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
                this.logger.LogWarning($"Received unregistered packet {packetId}");
            }
        }
    }
}
