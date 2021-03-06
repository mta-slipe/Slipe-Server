using System;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Server.Elements;
using Newtonsoft.Json;

namespace SlipeServer.Server.Behaviour
{
    /// <summary>
    /// Behaviour responsbile for logging triggered MTA Lua events
    /// </summary>
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
