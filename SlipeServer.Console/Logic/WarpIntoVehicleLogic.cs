using Microsoft.Extensions.Logging;
using SlipeServer.Console.Elements;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace SlipeServer.Console.Logic;

public class WarpIntoVehicleLogic
{
    private readonly ILogger logger;
    private readonly LuaEventService luaEventService;
    private readonly MtaServer server;

    public WarpIntoVehicleLogic(
        MtaServer<CustomPlayer> server,
        ILogger logger,
        CommandService commandService,
        LuaEventService luaEventService)
    {
        this.server = server;
        this.logger = logger;
        this.luaEventService = luaEventService;

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
