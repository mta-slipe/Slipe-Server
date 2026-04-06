using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Services;
using System.Numerics;

namespace SlipeServer.Console.Logic;

public class PedTestLogic
{
    private readonly ICommandService commandService;
    private readonly Ped ped;

    public PedTestLogic(
        IMtaServer<CustomPlayer> server,
        ICommandService commandService
    )
    {
        this.commandService = commandService;

        this.ped = new Ped(Server.Elements.Enums.PedModel.Swat, new Vector3(12.5f, 0, 3))
            .AssociateWith(server);

        this.SetupTestCommands();
    }

    private void SetupTestCommands()
    {
        this.commandService.AddCommand("pedanim").Triggered += (source, args) =>
        {
            if (args.Arguments.Length < 2)
                return;

            this.ped.SetAnimation(args.Arguments[0], args.Arguments[1]);
        };

        this.commandService.AddCommand("inplacepedanim").Triggered += (source, args) =>
        {
            if (args.Arguments.Length < 2)
                return;

            this.ped.SetAnimation(args.Arguments[0], args.Arguments[1], updatesPosition: false);
        };

        this.commandService.AddCommand("pedanimprogress").Triggered += (source, args) =>
        {
            if (args.Arguments.Length < 1)
                return;

            var progress = 0f;
            if (args.Arguments.Length > 1)
                if (!float.TryParse(args.Arguments[1], out progress))
                    progress = 0;

            this.ped.SetAnimationProgress(args.Arguments[0], progress);
        };

        this.commandService.AddCommand("pedanimspeed").Triggered += (source, args) =>
        {
            if (args.Arguments.Length < 1)
                return;

            var speed = 1f;
            if (args.Arguments.Length > 1)
                if (!float.TryParse(args.Arguments[1], out speed))
                    speed = 1;

            this.ped.SetAnimationSpeed(args.Arguments[0], speed);
        };

        this.commandService.AddCommand("stoppedanim").Triggered += (source, args) =>
        {
            this.ped.StopAnimation();
        };

        this.commandService.AddCommand("mygravity").Triggered += (source, args) =>
        {
            if (args.Arguments.Length < 1)
                return;

            if (!float.TryParse(args.Arguments[0], out float gravity))
                gravity = 1;

            args.Player.Gravity = gravity;
        };

        this.commandService.AddCommand("reload").Triggered += (source, args) =>
        {
            args.Player.ReloadWeapon();
        };

        this.commandService.AddCommand("pedheadless").Triggered += (source, args) =>
        {
            this.ped.IsHeadless = !this.ped.IsHeadless;
        };

        this.commandService.AddCommand("pedmoveanim").Triggered += (source, args) =>
        {
            if (args.Arguments.Length < 1 || !int.TryParse(args.Arguments[0], out int animId))
                return;

            this.ped.MoveAnimation = (PedMoveAnimation)animId;
        };

        this.commandService.AddCommand("pedchoking").Triggered += (source, args) =>
        {
            this.ped.IsChoking = !this.ped.IsChoking;
        };

        this.commandService.AddCommand("pedonfire").Triggered += (source, args) =>
        {
            this.ped.IsOnFire = !this.ped.IsOnFire;
        };

        this.commandService.AddCommand("pedgangdriveby").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            if (this.ped.Vehicle == null)
                this.ped.WarpIntoVehicle(args.Player.Vehicle, 1);

            this.ped.AddWeapon(Server.Enums.WeaponId.M4, 1000, true);
            this.ped.IsDoingGangDriveby = !this.ped.IsDoingGangDriveby;
        };

        this.commandService.AddCommand("driveby").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            args.Player.IsDoingGangDriveby = !args.Player.IsDoingGangDriveby;
        };

        this.commandService.AddCommand("walkstyle").Triggered += (source, args) =>
        {
            if (args.Arguments.Length < 1)
                return;

            if (!Enum.TryParse<PedMoveAnimation>(args.Arguments[0], out var value))
                return;

            args.Player.MoveAnimation = value;
        };
    }
}
