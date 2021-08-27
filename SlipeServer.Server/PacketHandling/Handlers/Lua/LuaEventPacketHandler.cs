using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Repositories;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Lua
{
    public class LuaEventPacketHandler : IPacketHandler<LuaEventPacket>
    {
        private readonly IElementRepository elementRepository;
        private readonly ILogger logger;
        private readonly MtaServer server;

        public PacketId PacketId => PacketId.PACKET_ID_LUA_EVENT;

        public LuaEventPacketHandler(
            IElementRepository elementRepository,
            ILogger logger,
            MtaServer server
        )
        {
            this.elementRepository = elementRepository;
            this.logger = logger;
            this.server = server;
        }

        public void HandlePacket(Client client, LuaEventPacket packet)
        {
            var element = this.elementRepository.Get(packet.ElementId);
            if (element == null)
            {
                this.logger.LogWarning($"'{packet.Name}' event triggered on non-existant element {packet.ElementId}");
                return;
            }

            this.server.HandleLuaEvent(new Events.LuaEvent()
            {
                Player = client.Player,
                Name = packet.Name,
                Source = element,
                Parameters = packet.LuaValues.ToArray()
            });
        }
    }
}
