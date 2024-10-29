using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.Example.Elements;
using SlipeServer.Example.LuaValues;
using SlipeServer.Example.Services;
using SlipeServer.LuaControllers;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System;
using System.Numerics;

namespace SlipeServer.Example.Controllers;

public class GenericDto<T> where T : class
{
    public T Value { get; set; } = null!;
}

[LuaController("SlipeServer.Test.")]
public class TestController : BaseLuaController<CustomPlayer>
{
    private readonly ChatBox chatBox;
    private readonly TestService testService;
    private readonly ILogger logger;
    private readonly IServiceProvider serviceProvider;
    private readonly IServiceScope serviceScope;

    public TestController(IServiceProvider serviceProvider, ChatBox chatBox, ILogger logger)
    {
        this.serviceScope = serviceProvider.CreateScope();
        this.serviceProvider = this.serviceScope.ServiceProvider;
        this.chatBox = chatBox;
        this.testService = this.serviceProvider.GetRequiredService<TestService>();
        this.logger = logger;
        this.logger.LogInformation("Instantiating {type}", typeof(TestController));
    }

    [LuaEvent("BlurLevel")]
    public void HandleblurLevel(int level)
    {
        this.testService.HandleBlurLevel(this.Context.Player, level);
    }

    [LuaEvent("Ui")]
    public void OutputUiStates(UiActiveStateLuaValue uiStates)
    {
        this.chatBox.Output($"{this.Context.Player.Name}'s UI states are:");
        this.chatBox.Output($"  IsChatBoxInputActive: {uiStates.IsChatBoxInputActive}");
        this.chatBox.Output($"  IsConsoleActive: {uiStates.IsConsoleActive}");
        this.chatBox.Output($"  IsDebugViewActive: {uiStates.IsDebugViewActive}");
        this.chatBox.Output($"  IsMainMenuActive: {uiStates.IsMainMenuActive}");
        this.chatBox.Output($"  IsMTAWindowActive: {uiStates.IsMTAWindowActive}");
        this.chatBox.Output($"  IsTransferBoxActive: {uiStates.IsTransferBoxActive}");
    }

    [Timed(10_000)]
    public void EveryTenSeconds()
    {
        this.logger.LogInformation("{name} method called on {type} at {time}", nameof(EveryTenSeconds), nameof(TestController), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
    }

    [Timed(60_000)]
    public void EveryMinute()
    {
        this.logger.LogInformation("{name} method called on {type} at {time}", nameof(EveryMinute), nameof(TestController), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
    }

    public string GetServerTime()
    {
        return DateTime.Now.ToString();
    }

    public void ThrowError()
    {
        throw new Exception();
    }

    public void OutputCursorPosition(Vector2 position)
    {
        this.chatBox.Output($"{this.Context.Player.Name}'s cursor is at  {position}");
    }

    public void EnumTest(WeaponId weapon, BodyPart bodyPart)
    {
        this.chatBox.Output($"Weapon : {weapon}");
        this.chatBox.Output($"BodyPart : {bodyPart}");
    }

    public void GenericTest(GenericDto<string> dto)
    {
        this.chatBox.Output(dto.Value);
    }
}
