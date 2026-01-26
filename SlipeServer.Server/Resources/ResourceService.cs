using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.Providers;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.Resources;

/// <summary>
/// Service that allows simple ways to start and stop resources (for all players)
/// </summary>
public class ResourceService
{
    private readonly IMtaServer server;
    private readonly RootElement root;
    private readonly IResourceProvider resourceProvider;

    private readonly List<Resource> startedResources = [];

    public IReadOnlyCollection<Resource> StartedResources => this.startedResources.AsReadOnly();

    public ResourceService(IMtaServer server, RootElement root, IResourceProvider resourceProvider)
    {
        this.server = server;
        this.root = root;
        this.resourceProvider = resourceProvider;

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
        if (!this.startedResources.Any(r => r.Name == name))
        {
            var resource = this.resourceProvider.GetResource(name);
            resource.Start();
            this.startedResources.Add(resource);

            return resource;
        }
        return null;
    }

    public void StopResource(string name)
    {
        var resource = this.startedResources.Single(r => r.Name == name);
        this.startedResources.Remove(resource);
        resource.Stop();
    }

    public void StopResource(Resource resource)
    {
        this.startedResources.Remove(resource);
        resource.Stop();
    }
}
