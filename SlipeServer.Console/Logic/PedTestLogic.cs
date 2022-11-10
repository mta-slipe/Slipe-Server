using SlipeServer.Console.Elements;
using SlipeServer.Packets.Enums;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Console.Logic;

public class PedTestLogic
{
    private readonly MtaServer<CustomPlayer> server;
    private readonly ChatBox chatBox;
    private readonly CommandService commandService;
    private readonly Ped ped;

    public PedTestLogic(
        MtaServer<CustomPlayer> server,
        ChatBox chatBox,
        CommandService commandService
    )
    {
        this.server = server;
        this.chatBox = chatBox;
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
    }
}
