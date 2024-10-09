using Microsoft.Extensions.Logging;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting;

public class ServerResourceService
{
    private readonly IReadOnlyDictionary<string, IScriptingService> scriptingServicesByLanguage;
    private readonly ILogger<ServerResourceService> logger;
    private readonly IResourceProvider resourceProvider;
    private readonly IScriptEventRuntime scriptEventRuntime;
    private readonly List<ServerResource> serverResources = [];
    private readonly List<ServerResource> startedResources = [];

    public event Action<Resource>? Started;
    public event Action<Resource>? Stopped;

    public ServerResourceService(IEnumerable<IScriptingService> scriptingServices, ILogger<ServerResourceService> logger, IResourceProvider resourceProvider, IScriptEventRuntime scriptEventRuntime)
    {
        this.scriptingServicesByLanguage = scriptingServices
            .ToDictionary(x => x.Language, x => x);
        this.logger = logger;
        this.resourceProvider = resourceProvider;
        this.scriptEventRuntime = scriptEventRuntime;
    }

    public ServerResource? GetResource(string name) => serverResources.FirstOrDefault(x => x.Name == name);

    public ServerResource Create(ServerResourceFiles serverResourceFiles, Resource resource)
    {
        var serverResource = new ServerResource(resource, serverResourceFiles.Name, serverResourceFiles.Chunks, this.scriptingServicesByLanguage);

        this.serverResources.Add(serverResource);

        return serverResource;
    }

    public ResourceState GetResourceState(string name) => this.startedResources.Any(x => x.Name == name) ? ResourceState.Running : ResourceState.Loaded;

    public bool StartResource(string name)
    {
        if (!this.startedResources.Any(r => r.Name == name))
        {
            var serverResource = this.serverResources.Single(r => r.Name == name);
            serverResource.Start();
            this.startedResources.Add(serverResource);
            Started?.Invoke(serverResource.Resource);
            //this.scriptEventRuntime.TriggerEvent("onResourceStart", root, root, serverResource.Resource);
            return true;
        }
        return false;
    }

    public bool StopResource(string name)
    {
        if (this.startedResources.Any(r => r.Name == name))
        {
            var resource = this.startedResources.Single(r => r.Name == name);
            this.startedResources.Remove(resource);
            resource.Stop();
            Stopped?.Invoke(resource.Resource);
            return true;
        }
        return false;
    }

}
