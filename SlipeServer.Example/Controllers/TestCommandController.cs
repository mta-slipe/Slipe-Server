﻿using Microsoft.Extensions.Logging;
using SlipeServer.LuaControllers;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System.Reflection;

namespace SlipeServer.Example.Controllers;

internal class NoAccessAttribute : Attribute;

[CommandController()]
public class TestCommandController : BaseCommandController<CustomPlayer>
{
    private readonly ChatBox chatBox;
    private readonly IElementCollection elementCollection;
    private readonly BanService banService;
    private readonly ILogger logger;

    public TestCommandController(ChatBox chatBox, IElementCollection elementCollection, BanService banService, ILogger logger)
    {
        this.chatBox = chatBox;
        this.elementCollection = elementCollection;
        this.banService = banService;
        this.logger = logger;
        this.logger.LogInformation("Instantiating {type}", typeof(TestController));
    }

    protected override void Invoke(Action next)
    {
        try
        {
            if (this.Context.MethodInfo.GetCustomAttribute<NoAccessAttribute>() != null)
            {
                this.chatBox.OutputTo(this.Context.Player, $"You can not access command {this.Context.Command}");
            } else
            {
                next();
            }
        }
        catch (Exception ex)
        {
            this.chatBox.OutputTo(this.Context.Player, $"Failed to execute command {this.Context.Command}");
        }
    }

    public void Chat(IEnumerable<string> words)
    {
        this.chatBox.OutputTo(this.Context.Player, string.Join(' ', words));
    }

    [NoAccess]
    public void NoAccess()
    {
        this.chatBox.OutputTo(this.Context.Player, "You have accessed command with NoAccess attribute!");
    }

    public void Oops()
    {
        throw new Exception("oops");
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

    public void BanPlayer(string name)
    {
        var matchingPlayers = this.elementCollection
            .GetByType<Player>()
            .Where(x => x.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase));

        if (matchingPlayers.Count() == 1)
        {
            var player = matchingPlayers.Single();
            this.banService.AddBan(player.Client.Serial, null, DateTime.UtcNow + TimeSpan.FromSeconds(30), "Testing purposes.");
        }
    }
}
