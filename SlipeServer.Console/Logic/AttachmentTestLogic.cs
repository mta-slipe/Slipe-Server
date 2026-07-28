using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace SlipeServer.Console.Logic;

public class AttachmentTestLogic
{
    public AttachmentTestLogic(ICommandService commandService)
    {
        commandService.AddCommand("attachtovehicle").Triggered += SetAttachToVehicle;
        commandService.AddCommand("detach").Triggered += DetachFromVehicle; ;
    }

    private void DetachFromVehicle(object? sender, CommandTriggeredEventArgs e)
        => e.Player.DetachFrom();

    private void SetAttachToVehicle(object? sender, CommandTriggeredEventArgs e)
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
            sender.AttachTo(vehicle, new System.Numerics.Vector3(0, 0, 3));
    }
}
