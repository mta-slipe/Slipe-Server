using Microsoft.Extensions.Logging;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace SlipeServer.Console.Logic;

public class WarpIntoVehicleLogic
{
    private readonly ILogger logger;

    public WarpIntoVehicleLogic(
        ILogger logger,
        CommandService commandService)
    {
        this.logger = logger;

        commandService.AddCommand("warpinvehicle").Triggered += SetWarpIntoVehicle;
    }

    private void SetWarpIntoVehicle(object? sender, CommandTriggeredEventArgs e)
    {
        var player = (CustomPlayer)e.Player;
        player.IsClickingVehicle = true;
        player.SetIsCursorShowing(true);

        player.CursorClicked += HandlePlayerCursorClick;
    }

    private void HandlePlayerCursorClick(Player sender, PlayerCursorClickedEventArgs e)
    {
        var player = (CustomPlayer)sender;

        player.SetIsCursorShowing(false);
        player.CursorClicked -= HandlePlayerCursorClick;

        if (e.Element is Vehicle vehicle)
        {
            sender.WarpIntoVehicle(vehicle);
            this.logger.LogInformation(
                "{player} warped into a {vehicle} (id: {vehicleId})",
                player.Name,
                (VehicleModel)vehicle.Model,
                vehicle.Id
            );
        }
    }
}
