using SlipeServer.Console.Elements;
using SlipeServer.Console.LuaValues;
using SlipeServer.Console.Services;
using SlipeServer.LuaControllers;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server.Services;
using System;
using System.Numerics;

namespace SlipeServer.Console.Controllers;

[LuaController("SlipeServer.Test.")]
public class TestController : BaseLuaController<CustomPlayer>
{
    private readonly ChatBox chatBox;
    private readonly TestService testService;

    public TestController(ChatBox chatBox, TestService testService)
    {
        this.chatBox = chatBox;
        this.testService = testService;
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
}
