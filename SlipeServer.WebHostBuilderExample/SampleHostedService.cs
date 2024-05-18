using SlipeServer.Console.Logic;
using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System.Numerics;

public class SampleHostedService : IHostedService
{
    private readonly MtaServer mtaServer;
    private readonly ChatBox chatBox;

    public SampleHostedService(MtaServer mtaServer, ChatBox chatBox)
    {
        this.mtaServer = mtaServer;
        this.chatBox = chatBox;
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
