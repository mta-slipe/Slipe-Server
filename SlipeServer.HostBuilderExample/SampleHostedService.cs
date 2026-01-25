using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server.Services;
using System.Numerics;

namespace SlipeServer.HostBuilderExample;

public class SampleHostedService(MtaServer mtaServer, IChatBox chatBox) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        mtaServer.PlayerJoined += HandlePlayerJoined;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        mtaServer.PlayerJoined -= HandlePlayerJoined;
        return Task.CompletedTask;
    }

    private void HandlePlayerJoined(Player player)
    {
        var client = player.Client;

        chatBox.Output($"{player.Name} ({client.Version}) ({client.Serial}) has joined the server!");

        player.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
        player.Health = 50;
        player.Alpha = 100;
        player.Camera.Target = player;
        player.Camera.Fade(CameraFade.In);
    }
}
