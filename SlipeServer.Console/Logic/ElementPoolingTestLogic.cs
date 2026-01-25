using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling;
using SlipeServer.Server.Services;
using System.Numerics;

namespace SlipeServer.Console.Logic;

public class ElementPoolingTestLogic
{
    private readonly ElementPool<WorldObject> elementPool;
    private readonly Stack<WorldObject> objects;
    private readonly ILogger logger;
    private readonly MtaServer server;

    public ElementPoolingTestLogic(ICommandService commandService, ILogger logger, MtaServer server)
    {
        this.logger = logger;
        this.server = server;

        this.elementPool = new();
        this.objects = new();

        commandService.AddCommand("more").Triggered += CreateElement;
        commandService.AddCommand("less").Triggered += DestroyElement;
    }

    private void CreateElement(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        var count = 1;
        if (e.Arguments.Length > 0)
            count = int.Parse(e.Arguments[0]);

        for (int i = 0; i < count; i++)
        {
            var element = this.elementPool.GetOrCreateElement(() =>
            {
                this.logger.LogInformation("New element created");
                return new WorldObject(321, Vector3.Zero);
            }).AssociateWith(this.server);
            element.Position = new Vector3(this.objects.Count, 0, 5);
            element.Scale = new Vector3(3);
            this.objects.Push(element);
        }
    }

    private void DestroyElement(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        var count = 1;
        if (e.Arguments.Length > 0)
            count = int.Parse(e.Arguments[0]);

        for (int i = 0; i < count; i++)
        {
            if (this.objects.TryPop(out var worldObject))
                worldObject.Destroy();
        }
    }
}
