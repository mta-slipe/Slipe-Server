using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Services;
using System.Linq;

namespace SlipeServer.Console.AdditionalResources.ResourceWithFeatures;

public class ResourceWithFeaturesLogic
{
    private readonly MtaServer server;
    private readonly LuaEventService luaEventService;
    private readonly ILogger logger;
    private readonly IElementCollection elementCollection;
    private readonly ResourceWithFeaturesResource resource;

    public ResourceWithFeaturesLogic(MtaServer server,
        LuaEventService luaEventService,
        ILogger logger,
        IElementCollection elementCollection)
    {
        this.server = server;
        this.luaEventService = luaEventService;
        this.logger = logger;
        this.elementCollection = elementCollection;
        server.PlayerJoined += HandlePlayerJoin;

        this.resource = this.server.GetAdditionalResource<ResourceWithFeaturesResource>();
    }

    private void HandlePlayerJoin(Player player)
    {
        this.resource.StartFor(player);
    }
}
