using Microsoft.Extensions.Logging;
using SlipeServer.Console.Elements;
using SlipeServer.Console.Services;
using SlipeServer.LuaControllers;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;

namespace SlipeServer.Console.Controllers;

[CommandController()]
public class TestCommandController : BaseCommandController<CustomPlayer>
{
    private readonly ChatBox chatBox;
    private readonly TestService testService;
    private readonly ILogger logger;

    public TestCommandController(ChatBox chatBox, TestService testService, ILogger logger)
    {
        this.chatBox = chatBox;
        this.testService = testService;
        this.logger = logger;
        this.logger.LogInformation("Instantiating {type}", typeof(TestController));
    }

    public void Ping()
    {
        this.chatBox.OutputTo(this.Context.Player, $"Your ping is {this.Context.Player.Client.Ping}.");
    }

    [Command("tp")]
    [Command("teleport")]
    public void Teleport(float x, float y, float z)
    {
        this.Context.Player.Position = new(x, y, z);
    }

    public void SpawnAt(ushort model, float x, float y, float z)
    {
        this.Context.Player.Spawn(new System.Numerics.Vector3(x, y, z), 0, model, 0, 0);
    }

    public void GiveWeapon(WeaponId weapon, ushort ammoCount = 100)
    {
        this.Context.Player.AddWeapon(weapon, ammoCount, true);
    }

    [NoCommand()]
    public void NoCommand()
    {
        this.chatBox.OutputTo(this.Context.Player, $"This should not run.");
    }
}
