using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Repositories;
using System.Linq;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class LuaEventQueueHandler : WorkerBasedQueueHandler
    {
        private readonly MtaServer server;
        private readonly IElementRepository elementRepository;
        private readonly ILogger logger;
        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[] { PacketId.PACKET_ID_LUA_EVENT };

        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_LUA_EVENT] = typeof(LuaEventPacket),
        };

        public LuaEventQueueHandler(MtaServer server, IElementRepository elementRepository, ILogger logger, int sleepInterval, int workerCount) 
            : base(sleepInterval, workerCount)
        {
            this.server = server;
            this.elementRepository = elementRepository;
            this.logger = logger;
        }

        protected override void HandlePacket(Client client, Packet packet)
        {
            try
            {
                switch (packet)
                {
                    case LuaEventPacket eventPacket:
                        var element = this.elementRepository.Get(eventPacket.ElementId);
                        if (element == null)
                        {
                            this.logger.LogWarning($"'{eventPacket.Name}' event triggered on non-existant element {eventPacket.ElementId}");
                            break;
                        }

                        this.server.HandleLuaEvent(new Events.LuaEvent()
                        {
                            Player = client.Player,
                            Name = eventPacket.Name,
                            Source = element,
                            Parameters = eventPacket.LuaValues.ToArray()
                        });

                        break;
                }
            }
            catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
            }
        }

    }
}
