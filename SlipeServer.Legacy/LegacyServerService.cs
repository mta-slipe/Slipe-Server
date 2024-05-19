using Microsoft.Extensions.Logging;
using SlipeServer.Lua;
using SlipeServer.Scripting;
using SlipeServer.Server;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.Resources.Providers;

namespace SlipeServer.Legacy;

internal sealed class LegacyServerService : IHostedService
{
    private readonly IResourceProvider resourceProvider;
    private readonly IEnumerable<IResourceInterpreter> resourceInterpreters;
    private readonly ILogger<LegacyServerService> logger;
    private readonly MtaServer mtaServer;
    private readonly ClientResourceService resourceService;
    private readonly ServerResourceService serverResourcesService;
    private readonly IScriptEventRuntime scriptEventRuntime;
    private readonly LuaService luaService;

    public LegacyServerService(IResourceProvider resourceProvider, IEnumerable<IResourceInterpreter> resourceInterpreters, ILogger<LegacyServerService> logger, MtaServer mtaServer, ClientResourceService resourceService, ServerResourceService serverResourcesService, IScriptEventRuntime scriptEventRuntime, LuaService luaService)
    {
        foreach (var resourceInterpreter in resourceInterpreters)
            resourceProvider.AddResourceInterpreter(resourceInterpreter);

        this.resourceProvider = resourceProvider;
        this.resourceInterpreters = resourceInterpreters;
        this.logger = logger;
        this.mtaServer = mtaServer;
        this.resourceService = resourceService;
        this.serverResourcesService = serverResourcesService;
        this.scriptEventRuntime = scriptEventRuntime;
        this.luaService = luaService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.scriptEventRuntime.LoadDefaultEvents();
        this.luaService.LoadDefaultDefinitions();

        this.resourceProvider.Refresh();
        var resources = this.resourceProvider.GetResources().ToArray();
        var serverResourcesFiles = this.resourceProvider.GetServerResourcesFiles().ToArray();

        foreach (var item in serverResourcesFiles)
        {
            this.serverResourcesService.Create(item);
        }

        this.logger.LogInformation("Resources: {loadedResourcesCount} loaded, {failedResourcesCount} failed", resources.Length, 0);
        if (this.mtaServer.HasPassword)
        {
            this.logger.LogInformation("Server password set to '{serverPassword}'", mtaServer.Password);
        }
        this.logger.LogInformation("Starting resources...");

        foreach (var startupResource in this.mtaServer.Configuration.StartupResources)
        {
            var resource = resources.FirstOrDefault(x => x.Name == startupResource.Name);
            if(resource != null)
            {
                this.resourceService.StartResource(startupResource.Name);
            } else
            {
                this.logger.LogWarning("ERROR: Couldn't find resource {resourceName}. Check it exists.", startupResource.Name);
            }
        }
        this.mtaServer.Start();
        this.logger.LogInformation("Server started and is ready to accept connections!");
        this.logger.LogInformation("To stop the server, type 'shutdown' or press Ctrl-C");
        this.logger.LogInformation("Type 'help' for a list of commands.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.mtaServer.Stop();
        return Task.CompletedTask;
    }
}
