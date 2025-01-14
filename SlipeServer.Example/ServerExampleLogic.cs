using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;

namespace SlipeServer.Example;

public class ServerExampleLogic
{
    private readonly CommandService commandService;
    private readonly ChatBox chatBox;

    public ServerExampleLogic(CommandService commandService, ChatBox chatBox, MtaServer mtaServer)
    {
        this.commandService = commandService;
        this.chatBox = chatBox;

        AddCommand("hello", player =>
        {
            this.chatBox.OutputTo(player, "Hello world");
        });

        AddCommand("spawndespawnvehicle", player =>
        {
            if (player.Vehicle == null)
            {
                var vehicle = new Vehicle(404, player.Position).AssociateWith(mtaServer);
                player.WarpIntoVehicle(vehicle);
            } else
            {
                player.Vehicle.Destroy();
            }
        });

        AddCommand("toggleControls", player =>
        {
            var controls = player.Controls;
            controls.ToggleAll(false);
            controls.ForwardsEnabled = true;
            this.chatBox.OutputTo(player, "Toggle");
        });

        AddVehiclesCommands();
    }

    private void AddVehiclesCommands()
    {
        AddCommand("myvehprintdamage", player =>
        {
            var vehicle = player.Vehicle!;

            this.chatBox.OutputTo(player, "List of damaged vehicles parts:");
            this.chatBox.OutputTo(player, "Doors:");
            foreach (var item in Enum.GetValues<Packets.Enums.VehicleDoor>())
            {
                var state = vehicle.GetDoorState(item);
                if(state != Packets.Enums.VehicleDoorState.ShutIntact)
                    this.chatBox.OutputTo(player, $" {item} - {state}");
            }

            this.chatBox.OutputTo(player, "Panels:");
            foreach (var item in Enum.GetValues<Packets.Enums.VehiclePanel>())
            {
                var state = vehicle.GetPanelState(item);
                if(state != Packets.Enums.VehiclePanelState.Undamaged)
                    this.chatBox.OutputTo(player, $" {item} - {state}");
            }

            this.chatBox.OutputTo(player, "Wheels:");
            foreach (var item in Enum.GetValues<Packets.Enums.VehicleWheel>())
            {
                var state = vehicle.GetWheelState(item);
                if(state != Packets.Enums.VehicleWheelState.Inflated)
                    this.chatBox.OutputTo(player, $" {item} - {state}");
            }
        });

        AddCommand("damagemyveh", player =>
        {
            var vehicle = player.Vehicle!;

            vehicle.SetDoorState(Packets.Enums.VehicleDoor.Hood, Packets.Enums.VehicleDoorState.Missing);
            vehicle.SetPanelState(Packets.Enums.VehiclePanel.FrontBumper, Packets.Enums.VehiclePanelState.Damaged3);
            vehicle.SetWheelState(Packets.Enums.VehicleWheel.FrontLeft, Packets.Enums.VehicleWheelState.FallenOff);
            this.chatBox.OutputTo(player, "Vehicle damaged!");
        });

        AddCommand("fixmyveh", player =>
        {
            var vehicle = player.Vehicle!;

            vehicle.Fix();
            this.chatBox.OutputTo(player, "Vehicle fixed");
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
