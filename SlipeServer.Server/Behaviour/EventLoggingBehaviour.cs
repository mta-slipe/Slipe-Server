using Microsoft.Extensions.Logging;
using System.Linq;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsbile for logging triggered MTA Lua events
/// </summary>
public class EventLoggingBehaviour
{
    public EventLoggingBehaviour(IMtaServer server, ILogger logger)
    {
        server.LuaEventTriggered += (luaEvent) =>
        {
            logger.LogInformation("The lua '{luaEventName}' event was triggered by {luaEventPlayerName} with variables:\n" +
                "{parameters}", luaEvent.Name, luaEvent.Player.Name, string.Join(", ", luaEvent.Parameters.Select(p => p.ToString())));
        };
    }
}
