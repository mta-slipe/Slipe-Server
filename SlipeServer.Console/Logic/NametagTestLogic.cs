using SlipeServer.Server;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System.Drawing;

namespace SlipeServer.Console.Logic;

public class NametagTestLogic
{
    private readonly Player nametagPlayer;

    public NametagTestLogic(IMtaServer server, ICommandService commandService)
    {
        this.nametagPlayer = new CustomPlayer(server.GetRequiredService<IExplosionService>(), server);
        var client = new FakeClient(this.nametagPlayer)
        {
            Serial = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB",
            IPAddress = System.Net.IPAddress.Parse("127.0.0.1")
        };
        this.nametagPlayer.Client = client;
        this.nametagPlayer.AssociateWith(server);

        this.nametagPlayer.Name = "NametagTester";
        this.nametagPlayer.Position = new System.Numerics.Vector3(-2, 0.5f, 3);
        this.nametagPlayer.Model = 9;
        this.nametagPlayer.IsNametagShowing = true;
        this.nametagPlayer.NametagColor = Color.White;

        server.HandlePlayerJoin(this.nametagPlayer);

        commandService.AddCommand("togglenametag").Triggered += ToggleNametag;
        commandService.AddCommand("nametagtext").Triggered += SetNametagText;
        commandService.AddCommand("randomcolor").Triggered += RandomiseNametagColor;
    }

    private void ToggleNametag(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        this.nametagPlayer.IsNametagShowing = !this.nametagPlayer.IsNametagShowing;
    }

    private void SetNametagText(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        if (!e.Arguments.Any())
            return;

        this.nametagPlayer.NametagText = e.Arguments[0];
    }

    private void RandomiseNametagColor(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        var random = new Random();
        var bytes = new byte[3];
        random.NextBytes(bytes);
        var color = Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);
        this.nametagPlayer.NametagColor = color;
    }


}
