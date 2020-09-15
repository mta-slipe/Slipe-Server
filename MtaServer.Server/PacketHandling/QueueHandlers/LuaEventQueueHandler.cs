using MtaServer.Packets.Enums;
using MtaServer.Packets.Definitions.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MtaServer.Server.Elements;
using MtaServer.Packets.Lua.Event;
using MtaServer.Server.Repositories;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace MtaServer.Server.PacketHandling.QueueHandlers
{
    public class LuaEventQueueHandler : WorkerBasedQueueHandler
    {
        private readonly MtaServer server;
        private readonly IElementRepository elementRepository;
        private readonly ILogger logger;

        public LuaEventQueueHandler(MtaServer server, IElementRepository elementRepository, ILogger logger, int sleepInterval, int workerCount) 
            : base(sleepInterval, workerCount)
        {
            this.server = server;
            this.elementRepository = elementRepository;
            this.logger = logger;
        }

        protected override void HandlePacket(PacketQueueEntry queueEntry)
        {
            switch (queueEntry.PacketId)
            {
                case PacketId.PACKET_ID_LUA_EVENT:
                    LuaEventPacket eventPacket = new LuaEventPacket();
                    eventPacket.Read(queueEntry.Data);

                    var element = this.elementRepository.Get(eventPacket.ElementId);
                    if (element == null)
                    {
                        this.logger.LogWarning($"'{eventPacket.Name}' event triggered on non-existant element {eventPacket.ElementId}");
                        break;
                    }

                    this.server.HandleLuaEvent(new Events.LuaEvent()
                    {
                        Player = queueEntry.Client.Player,
                        Name = eventPacket.Name,
                        Source = element,
                        Parameters = eventPacket.LuaValues.ToArray()
                    });

                    break;
            }
        }

    }
}
