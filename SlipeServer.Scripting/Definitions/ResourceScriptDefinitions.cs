using SlipeServer.Server;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using System.Linq;

namespace SlipeServer.Scripting.Definitions;

public class ResourceScriptDefinitions(IMtaServer server)
{
    [ScriptFunctionDefinition("startResource")]
    public bool StartResource(
        Resource resource,
        bool persistent = false,
        bool startIncludedResources = true,
        bool loadServerConfigs = true,
        bool loadMaps = true,
        bool loadServerScripts = true,
        bool loadHtml = true,
        bool loadClientConfigs = true,
        bool loadClientScripts = true,
        bool loadFiles = true)
    {
        var resourceService = server.GetService<IResourceService>();
        return resourceService?.StartResource(resource.Name) != null;
    }

    [ScriptFunctionDefinition("stopResource")]
    public bool StopResource(Resource resource)
    {
        var resourceService = server.GetService<IResourceService>();
        if (resourceService == null || !resourceService.StartedResources.Any(r => r.Name == resource.Name))
            return false;

        resourceService.StopResource(resource.Name);
        return true;
    }

    [ScriptFunctionDefinition("restartResource")]
    public bool RestartResource(
        Resource resource,
        bool persistent = false,
        bool configs = true,
        bool maps = true,
        bool scripts = true,
        bool html = true,
        bool clientConfigs = true,
        bool clientScripts = true,
        bool clientFiles = true)
    {
        var resourceService = server.GetService<IResourceService>();
        if (resourceService == null || !resourceService.StartedResources.Any(r => r.Name == resource.Name))
            return false;

        resourceService.StopResource(resource.Name);
        return resourceService.StartResource(resource.Name) != null;
    }

    [ScriptFunctionDefinition("getResourceState")]
    public string GetResourceState(Resource resource)
    {
        var resourceService = server.GetService<IResourceService>();
        if (resourceService?.StartedResources.Any(r => r.Name == resource.Name) == true)
            return "running";

        return "loaded";
    }

    [ScriptFunctionDefinition("getResources")]
    public Resource[] GetResources()
    {
        var resourceProvider = server.GetService<IResourceProvider>();
        return resourceProvider == null ? [] : [.. resourceProvider.GetResources()];
    }
}
