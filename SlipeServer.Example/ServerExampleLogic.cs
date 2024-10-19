using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System.Numerics;

namespace SlipeServer.Example;

public class ServerExampleLogic
{
    private readonly CommandService commandService;
    private readonly ChatBox chatBox;
    private readonly MtaServer mtaServer;

    public ServerExampleLogic(CommandService commandService, ChatBox chatBox, MtaServer mtaServer)
    {
        this.commandService = commandService;
        this.chatBox = chatBox;
        this.mtaServer = mtaServer;
        AddCommand("hello", player =>
        {
            this.chatBox.OutputTo(player, "Hello world");
        });
        AddCommand("network", player =>
        {
            player.ExceptionThrown += (sender, ex) =>
            {
                Console.WriteLine("Exception thrown on player: {0}", ex);
            };

            player.VehicleChanged += (Ped sender, Server.Elements.Events.ElementChangedEventArgs<Ped, Vehicle?> args) =>
            {
                throw new InvalidOperationException("Hello your computer has network");
            };

            var veh = new Vehicle(602, new Vector3(0, 0, 15)).AssociateWith(this.mtaServer);
            player.WarpIntoVehicle(veh);
        });
    }

    private void AddCommand(string command, Action<Player> callback)
    {
        this.commandService.AddCommand(command).Triggered += (object? sender, Server.Events.CommandTriggeredEventArgs e) =>
        {
            callback(e.Player);
        };
    }
}
