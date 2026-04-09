using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;

namespace SlipeServer.DropInReplacement.MixedResources.Behaviour;

public class DropInReplacementResourceService : IDropInReplacementResourceService
{
    private readonly IMtaServer server;
    private readonly IResourceProvider resourceProvider;
    private readonly ILogger<DropInReplacementResourceProvider> logger;
    private readonly IDropInReplacementResourceLuaService luaResourceService;
    private readonly List<Resource> startedResources = [];

    public IReadOnlyCollection<Resource> StartedResources => this.startedResources.AsReadOnly();

    public DropInReplacementResourceService(
        IMtaServer server, 
        IResourceProvider resourceProvider, 
        ILogger<DropInReplacementResourceProvider> logger, 
        IDropInReplacementResourceLuaService luaResourceService)
    {
        this.server = server;
        this.resourceProvider = resourceProvider;
        this.logger = logger;
        this.luaResourceService = luaResourceService;

        this.server.PlayerJoined += HandlePlayerJoin;
    }

    private void HandlePlayerJoin(Player player)
    {
        foreach (var resource in this.startedResources)
        {
            resource.StartFor(player);
        }
    }

    public Resource? StartResource(string name)
    {
        logger.LogInformation("Starting {resource}", name);

        if (!this.startedResources.Any(r => r.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
        {
            var resource = this.resourceProvider.GetResource(name);
            resource.Start();
            this.startedResources.Add(resource);

            if (resource is MixedResource mixedResource)
                luaResourceService.StartLuaResource(mixedResource);
            else
                logger.LogWarning("Resource {resource} does is not a valid MixedResource", name);

            logger.LogInformation("Started {resource}", name);

            return resource;
        }

        return null;
    }

    public void StopResource(string name)
    {
        logger.LogInformation("Stopping {resource}", name);

        var resource = this.startedResources.Single(r => string.Equals(r.Name, name, StringComparison.InvariantCultureIgnoreCase));
        this.startedResources.Remove(resource);
        resource.Stop();

        if (resource is MixedResource mixedResource)
            luaResourceService.StopLuaResource(mixedResource);
        else
            logger.LogWarning("Resource {resource} does is not a valid MixedResource", name);

        logger.LogInformation("Stopped {resource}", name);
    }

    public void StopResource(Resource resource)
    {
        logger.LogInformation("Stopped {resource}", resource.Name);

        this.startedResources.Remove(resource);
        resource.Stop();
    }

    public void RestartResource(string name)
    {
        logger.LogInformation("Stopped {resource}", name);

        StopResource(name);
        StartResource(name);
    }
}

public interface IDropInReplacementResourceService
{
    Resource? StartResource(string name);
    void StopResource(string name);
    void StopResource(Resource resource);
    void RestartResource(string name);
}
