using Microsoft.Extensions.Logging;
using SlipeServer.Scripting;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;

namespace SlipeServer.Legacy;

internal sealed class LegacyConsoleCommandsService : IHostedService
{
    private readonly ILogger<LegacyConsoleCommandsService> logger;
    private readonly InteractiveServerConsole serverConsole;
    private readonly IResourceProvider resourceProvider;
    private readonly ClientResourceService clientResourceService;
    private readonly ServerResourceService serverResourcesService;

    public LegacyConsoleCommandsService(ILogger<LegacyConsoleCommandsService> logger, InteractiveServerConsole serverConsole, IResourceProvider resourceProvider, ClientResourceService clientResourceService, ServerResourceService serverResourcesService)
    {
        this.logger = logger;
        this.serverConsole = serverConsole;
        this.resourceProvider = resourceProvider;
        this.clientResourceService = clientResourceService;
        this.serverResourcesService = serverResourcesService;
    }

    private void HandleStartCommand(string name)
    {
        var serverResource = this.serverResourcesService.GetResource(name);

        if(serverResource == null)
        {
            this.logger.LogInformation("* Syntax: start <resource-name>");
            return;
        }

        try
        {
            serverResource.Start();
        }
        catch(Exception ex)
        {
            this.logger.LogError(ex, "Failed to start resource {resourceName}", name);
        }
    }
    
    private void HandleStopCommand(string name)
    {
        var resource = this.serverResourcesService.GetResource(name);

        if(resource == null)
        {
            this.logger.LogInformation("* Syntax: stop <resource-name>");
            return;
        }

        try
        {
            resource.Stop();
        }
        catch(Exception ex)
        {
            this.logger.LogError(ex, "Failed to stop resource {resourceName}", name);
        }
    }
    
    private void HandleRestartCommand(string name)
    {
        var resource = this.serverResourcesService.GetResource(name);

        if(resource == null)
        {
            this.logger.LogInformation("* Syntax: stop <resource-name>");
            return;
        }

        try
        {
            resource.Stop();
        }
        catch(Exception ex)
        {
            this.logger.LogError(ex, "Failed to stop resource {resourceName}", name);
        }
    }
    
    private void HandleRefreshCommand(string name)
    {
        var resource = this.serverResourcesService.GetResource(name);

        if(resource == null)
        {
            this.logger.LogInformation("* Syntax: stop <resource-name>");
            return;
        }

        try
        {
            this.resourceProvider.Refresh();
        }
        catch(Exception ex)
        {
            this.logger.LogError(ex, "Failed to stop resource {resourceName}", name);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.serverConsole.AddCommand("start", HandleStartCommand);
        this.serverConsole.AddCommand("stop", HandleStopCommand);
        this.serverConsole.AddCommand("restart", HandleRestartCommand);
        this.serverConsole.AddCommand("refresh", HandleRefreshCommand);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
