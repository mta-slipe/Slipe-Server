using SlipeServer.Example.Elements;
using SlipeServer.Packets.Enums;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System;
using System.Drawing;
using System.Linq;

namespace SlipeServer.Example.Logic;

public class VehicleTestLogic
{
    private readonly MtaServer<CustomPlayer> server;
    private readonly ChatBox chatBox;
    private readonly CommandService commandService;

    public VehicleTestLogic(
        MtaServer<CustomPlayer> server,
        ChatBox chatBox,
        CommandService commandService
    )
    {
        this.server = server;
        this.chatBox = chatBox;
        this.commandService = commandService;
        SetupTestCommands();
    }

    private void SetupTestCommands()
    {
        this.commandService.AddCommand("resethandling").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            args.Player.Vehicle.Handling = null;
            this.chatBox.OutputTo(args.Player, "Your handling has been reset to the model's defaults", Color.DarkOliveGreen);
        };

        this.commandService.AddCommand("quicker").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            var handling = args.Player.Vehicle.AppliedHandling;

            handling.MaxVelocity *= 2;
            handling.EngineAcceleration *= 2;

            args.Player.Vehicle.Handling = handling;
            this.chatBox.OutputTo(args.Player, "Your engine is now twice as fast", Color.DarkOliveGreen);
        };

        this.commandService.AddCommand("nocollisiondamage").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            var handling = args.Player.Vehicle.AppliedHandling;

            handling.CollisionDamageMultiplier = 0;

            args.Player.Vehicle.Handling = handling;
            this.chatBox.OutputTo(args.Player, "Your vehicle now takes no collision damage", Color.DarkOliveGreen);
        };

        this.commandService.AddCommand("drivetype").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            var arg = args.Arguments.Any() ? args.Arguments[0] : "";

            var handling = args.Player.Vehicle.AppliedHandling;

            if (!Enum.TryParse<VehicleDriveType>(arg, out var driveType))
                driveType = VehicleDriveType.FrontWheelDrive;

            handling.DriveType = driveType;

            args.Player.Vehicle.Handling = handling;
            this.chatBox.OutputTo(args.Player, $"Your vehicle is now {driveType}", Color.DarkOliveGreen);
        };

        this.commandService.AddCommand("vh").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            this.chatBox.OutputTo(args.Player, $"Your vehicle has {(int)args.Player.Vehicle.Health} health");
        };

        this.commandService.AddCommand("repair").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            args.Player.Vehicle.Health = 1000;
            this.chatBox.OutputTo(args.Player, "Your vehicle has been repaired");
        };

        this.commandService.AddCommand("lights").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            args.Player.Vehicle.OverrideLights = args.Player.Vehicle.OverrideLights == VehicleOverrideLights.On ?
                VehicleOverrideLights.Off : VehicleOverrideLights.On;
        };

        this.commandService.AddCommand("vehiclehealth").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null || !args.Arguments.Any())
                return;

            if (!float.TryParse(args.Arguments[0], out float health))
                health = 500;

            args.Player.Vehicle.Health = health;
        };
    }
}
