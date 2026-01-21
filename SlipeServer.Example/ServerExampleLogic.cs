using SlipeServer.Server;
using SlipeServer.Server.Concepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Exceptions;
using SlipeServer.Server.Services;

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

        AddCommand("clienttask", async player =>
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var task = this.mtaServer.CreateClientTask(player, cts.Token);

            player.TriggerLuaEvent("testClientTask", player, task);

            try
            {
                await task;
            }
            catch (PlayerDisconnectedException e) // When player left the server
            {
                Console.WriteLine("Result: PlayerDisconnectedException");
            }
            catch (InvalidOperationException e) // When client sent invalid response
            {
                Console.WriteLine("Result: InvalidOperationException");
            }
            catch (ClientErrorException e) // When client on purpose rejected task
            {
                Console.WriteLine("Result: ClientErrorException");
            }
            catch (OperationCanceledException e) // Exceptin from cts from above
            {
                Console.WriteLine("Result: OperationCanceledException");
            }
            finally
            {
                this.chatBox.OutputTo(player, "Task completed");
            }

        });
    }

    private void AddCommand(string command, Action<Player> callback)
    {
        this.commandService.AddCommand(command).Triggered += (object? sender, Server.Events.CommandTriggeredEventArgs e) =>
        {
            callback(e.Player);
        };
    }

    private void AddCommand(string command, Func<Player, Task> callback)
    {
        this.commandService.AddCommand(command).Triggered += async (object? sender, Server.Events.CommandTriggeredEventArgs e) =>
        {
            try
            {
                await callback(e.Player);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        };
    }
}
