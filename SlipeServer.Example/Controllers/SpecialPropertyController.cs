using SlipeServer.LuaControllers;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server.Services;
using static SlipeServer.Server.Services.IGameWorld;

namespace SlipeServer.Example.Controllers;

[CommandController()]
public class SpecialPropertyController(IGameWorld gameWorld) : BaseCommandController<CustomPlayer>
{
    [Command("specialprop")]
    public void Teleport(string property, bool state)
    {
        if (!Enum.TryParse<WorldSpecialProperty>(property, true, out var enumValue))
            return;

        Thread.Sleep(3);
        gameWorld.SetSpecialPropertyEnabled(enumValue, state);
    }
}
