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
    private readonly bool allowMissingIncludes;
    private readonly List<Resource> startedResources = [];

    public IReadOnlyCollection<Resource> StartedResources => this.startedResources.AsReadOnly();

    public DropInReplacementResourceService(
        IMtaServer server,
        IResourceProvider resourceProvider,
        ILogger<DropInReplacementResourceProvider> logger,
        IDropInReplacementResourceLuaService luaResourceService,
        bool allowMissingIncludes = false)
    {
        this.server = server;
        this.resourceProvider = resourceProvider;
        this.logger = logger;
        this.luaResourceService = luaResourceService;
        this.allowMissingIncludes = allowMissingIncludes;

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

        if (this.startedResources.Any(r => r.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            return null;

        return StartResourceInternal(name, new HashSet<string>(StringComparer.InvariantCultureIgnoreCase));
    }

    private Resource StartResourceInternal(string name, HashSet<string> startupStack)
    {
        if (this.startedResources.FirstOrDefault(r => r.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) is Resource alreadyStarted)
            return alreadyStarted;

        if (!startupStack.Add(name))
            throw new InvalidOperationException($"Circular resource include detected while starting '{name}'.");

        try
        {
            var resource = this.resourceProvider.GetResource(name);

            if (resource is MixedResource mixedResource)
                {
                    foreach (var include in mixedResource.IncludedResources)
                    {
                        try
                        {
                            StartResourceInternal(include.ResourceName, startupStack);
                        }
                        catch (KeyNotFoundException)
                        {
                            if (this.allowMissingIncludes)
                                this.logger.LogWarning("Included resource '{include}' (required by '{name}') was not found and will be skipped.", include.ResourceName, name);
                            else
                                throw new MissingResourceException(include.ResourceName, name);
                        }
                    }
                }

            this.ResourceStarting?.Invoke(resource);
            resource.Start();
            this.startedResources.Add(resource);

            if (resource is MixedResource startedMixedResource)
                this.luaResourceService.StartLuaResource(startedMixedResource);
            else
                this.logger.LogWarning("Resource {resource} does is not a valid MixedResource", name);

            this.ResourceStarted?.Invoke(resource);
            this.logger.LogInformation("Started {resource}", name);

            return resource;
        }
        finally
        {
            startupStack.Remove(name);
        }
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

        this.ResourceStopped?.Invoke(resource);
        logger.LogInformation("Stopped {resource}", name);
    }

    public void StopResource(Resource resource)
    {
        logger.LogInformation("Stopped {resource}", resource.Name);

        this.startedResources.Remove(resource);
        resource.Stop();
        this.ResourceStopped?.Invoke(resource);
    }

    public void RestartResource(string name)
    {
        logger.LogInformation("Restarting {resource}", name);

        StopResource(name);
        StartResource(name);
    }

    public event Action<Resource>? ResourceStarting;
    public event Action<Resource>? ResourceStarted;
    public event Action<Resource>? ResourceStopped;
}

public interface IDropInReplacementResourceService : IResourceService
{
    void RestartResource(string name);
}
