﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.Example.Elements;
using SlipeServer.Example.Services;
using SlipeServer.LuaControllers;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server.Services;

namespace SlipeServer.Example.Controllers;

[LuaController(usesScopedEvents: false)]
public class SingletonTestController : BaseLuaController<CustomPlayer>
{
    private readonly ChatBox chatBox;
    private readonly TestService testService;
    private readonly ILogger logger;
    private readonly IServiceProvider serviceProvider;
    private readonly IServiceScope serviceScope;

    public SingletonTestController(IServiceProvider serviceProvider, ChatBox chatBox, ILogger logger)
    {
        this.serviceScope = serviceProvider.CreateScope();
        this.serviceProvider = this.serviceScope.ServiceProvider;
        this.chatBox = chatBox;
        this.testService = this.serviceProvider.GetRequiredService<TestService>();
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
