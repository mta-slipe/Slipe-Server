using SlipeServer.Console.Logic;
using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System.Numerics;

namespace SlipeServer.WebHostBuilderExample;

public class SampleHostedService : IHostedService
{
    private readonly IMtaServer mtaServer;
    private readonly IChatBox chatBox;

    public SampleHostedService(IMtaServer mtaServer, IChatBox chatBox, ICommandService commandService)
    {
        this.mtaServer = mtaServer;
        this.chatBox = chatBox;
        commandService.AddCommand("startSample").Triggered += HandleStartSample;
        commandService.AddCommand("spawnVehicle").Triggered += HandleSpawnVehicleTriggered;
    }

    private void HandleSpawnVehicleTriggered(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        new Vehicle(404, e.Player.Position).AssociateWith(this.mtaServer);
    }

    private void HandleStartSample(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        this.chatBox.OutputTo(e.Player, "Starting sample resource");
        var resource = this.mtaServer.GetAdditionalResource<SampleResource>();
        resource.StartFor(e.Player);
        this.chatBox.OutputTo(e.Player, "Sample resource started");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.mtaServer.InstantiatePersistent<LuaTestLogic>();

        this.mtaServer.PlayerJoined += HandlePlayerJoined;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.mtaServer.PlayerJoined -= HandlePlayerJoined;
        return Task.CompletedTask;
    }

    private void HandlePlayerJoined(Player player)
    {
        var client = player.Client;

        this.chatBox.Output($"{player.Name} ({client.Version}) ({client.Serial}) has joined the server!");

        player.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
        player.Health = 50;
        player.Alpha = 100;
        player.Camera.Target = player;
        player.Camera.Fade(CameraFade.In);
    }
}
