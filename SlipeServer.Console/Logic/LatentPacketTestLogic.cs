using Microsoft.Extensions.Logging;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace SlipeServer.Console.Logic;

public class LatentPacketTestLogic
{
    private readonly ILogger logger;

    public LatentPacketTestLogic(
        ILuaEventService luaService,
        ILogger logger
    )
    {
        this.logger = logger;

        luaService.AddEventHandler("exampleLatentTrigger", this.HandleLatentEvent);
    }

    private void HandleLatentEvent(LuaEvent luaEvent)
    {
        this.logger.LogInformation("exampleLatentTrigger received: {value}", luaEvent.Parameters[0].StringValue?.Length);
    }
}
