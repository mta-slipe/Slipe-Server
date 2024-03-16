using Microsoft.Extensions.Logging;
using SlipeServer.Example.Elements;
using SlipeServer.Example.Services;
using SlipeServer.LuaControllers;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server.Services;
using System;

namespace SlipeServer.Example.Controllers;


[LuaController(usesScopedEvents: false)]
public class SingletonTestController : BaseLuaController<CustomPlayer>
{
    private readonly ChatBox chatBox;
    private readonly TestService testService;
    private readonly ILogger logger;

    public SingletonTestController(ChatBox chatBox, TestService testService, ILogger logger)
    {
        this.chatBox = chatBox;
        this.testService = testService;
        this.logger = logger;
        this.logger.LogInformation("Instantiating {type}", typeof(SingletonTestController));
    }

    [Timed(10_000)]
    public void EveryTenSeconds()
    {
        this.logger.LogInformation("{name} method called on {type} at {time}", nameof(EveryTenSeconds), nameof(SingletonTestController), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
    }

    [Timed(60_000)]
    public void EveryMinute()
    {
        this.logger.LogInformation("{name} method called on {type} at {time}", nameof(EveryMinute), nameof(SingletonTestController), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
    }
}
