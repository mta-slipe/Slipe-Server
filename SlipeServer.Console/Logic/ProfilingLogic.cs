using SlipeServer.Console.Elements;
using SlipeServer.Net.Wrappers;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Console.Logic;

public class ProfilingLogic
{
    private readonly MtaServer<CustomPlayer> server;
    private readonly ChatBox chatBox;
    private readonly CommandService commandService;
    private readonly ProfilingNetWrapper profilingNetWrapper;

    public ProfilingLogic(
        MtaServer<CustomPlayer> server,
        ChatBox chatBox,
        CommandService commandService,
        ProfilingNetWrapper profilingNetWrapper
    )
    {
        this.server = server;
        this.chatBox = chatBox;
        this.commandService = commandService;
        this.profilingNetWrapper = profilingNetWrapper;

        this.SetupTestCommands();
    }

    private void SetupTestCommands()
    {
        SetupCommandTest();
        SetupColshapeTest();
    }

    private void SetupCommandTest()
    {
        var startCommand = this.commandService.AddCommand("commandprofile");

        void TriggerBreakpoint(object? sender, CommandTriggeredEventArgs args)
        {
            Debugger.Break();
        }
        startCommand.Triggered += TriggerBreakpoint;
    }

    private void SetupColshapeTest()
    {
        var startCommand = this.commandService.AddCommand("colshapetest");
        var stopCommand = this.commandService.AddCommand("stopcolshapetest");

        Marker marker;
        CollisionShape collisionShape;

        void StartTest(object? sender, CommandTriggeredEventArgs args)
        {
            var position = args.Player.Position;
            marker = new Marker(position, MarkerType.Arrow) { Color = Color.MediumAquamarine }.AssociateWith(this.server);
            collisionShape = new CollisionSphere(position, 1).AssociateWith(this.server);
            collisionShape.ElementEntered += (element) =>
            {
                if (element == args.Player)
                    args.Player.Kill();
            };

            this.profilingNetWrapper.TagIncomingPacket(Packets.Enums.PacketId.PACKET_ID_PLAYER_PURESYNC);
            this.profilingNetWrapper.TagOutgoingPacket(Packets.Enums.PacketId.PACKET_ID_PLAYER_WASTED);

            stopCommand.Triggered += StopTest;
            startCommand.Triggered -= StartTest;
        }

        void StopTest(object? sender, CommandTriggeredEventArgs args)
        {
            marker?.Destroy();
            collisionShape?.Destroy();

            stopCommand.Triggered -= StopTest;
            startCommand!.Triggered += StartTest;
        }

        startCommand.Triggered += StartTest;
    }
}
