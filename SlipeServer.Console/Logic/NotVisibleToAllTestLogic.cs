using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace SlipeServer.Console.Logic;

public class NotVisibleToAllTestLogic
{
    private readonly ILogger logger;
    private readonly WorldObject invisibleWorldObject;

    public NotVisibleToAllTestLogic(
        ILogger logger,
        CommandService commandService,
        MtaServer server)
    {
        this.logger = logger;

        this.invisibleWorldObject = new WorldObject(321, new(0, 0, 10))
        {
            Scale = new(10),
            IsVisibleToEveryone = false
        }.AssociateWith(server);

        commandService.AddCommand("createforme").Triggered += HandleCreateForMe;
        commandService.AddCommand("destroyforme").Triggered += HandleDestroyForMe;
    }

    private void HandleCreateForMe(object? sender, CommandTriggeredEventArgs e)
    {
        this.invisibleWorldObject.CreateFor(e.Player);
    }

    private void HandleDestroyForMe(object? sender, CommandTriggeredEventArgs e)
    {
        this.invisibleWorldObject.DestroyFor(e.Player);
    }
}
