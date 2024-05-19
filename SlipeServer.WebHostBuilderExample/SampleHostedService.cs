using SlipeServer.Console.Logic;
using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Services;
using SlipeServer.WebHostBuilderExample;
using System.Numerics;

public class SampleHostedService : IHostedService
{
    private readonly MtaServer mtaServer;
    private readonly ChatBox chatBox;
    private readonly IResourceProvider resourceProvider;

    public SampleHostedService(MtaServer mtaServer, ChatBox chatBox, CommandService commandService, IResourceProvider resourceProvider)
    {
        this.mtaServer = mtaServer;
        this.chatBox = chatBox;
        this.resourceProvider = resourceProvider;
        commandService.AddCommand("startSample").Triggered += HandleStartSample;
    }

    private void HandleStartSample(object? sender, SlipeServer.Server.Events.CommandTriggeredEventArgs e)
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
