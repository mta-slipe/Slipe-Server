using System;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;
using MtaServer.Packets.Definitions.Commands;
using MtaServer.Server.Elements;
using Newtonsoft.Json;

namespace MtaServer.Server.Behaviour
{
    public class EventLoggingBehaviour
    {
        public EventLoggingBehaviour(MtaServer server, ILogger? logger)
        {
            server.LuaEventTriggered += (luaEvent) =>
            {
                logger.LogInformation($"The lua '{luaEvent.Name}' event was triggered by {luaEvent.Player.Name} with variables:\n" +
                    $"{string.Join(", ", luaEvent.Parameters.Select(p => p.ToString()))}");
            };
        }
    }
}
