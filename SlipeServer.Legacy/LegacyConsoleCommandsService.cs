using Microsoft.Extensions.Logging;
using SlipeServer.Scripting;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Services;

namespace SlipeServer.Legacy;

internal sealed class LegacyConsoleCommandsService : IHostedService
{
    private readonly ILogger<LegacyConsoleCommandsService> logger;
    private readonly InteractiveServerConsole serverConsole;
    private readonly IResourceProvider resourceProvider;
    private readonly ClientResourceService clientResourceService;
    private readonly ServerResourceService serverResourcesService;
    private readonly ChatBox chatBox;

    public LegacyConsoleCommandsService(ILogger<LegacyConsoleCommandsService> logger, InteractiveServerConsole serverConsole, IResourceProvider resourceProvider, ClientResourceService clientResourceService, ServerResourceService serverResourcesService, ChatBox chatBox)
    {
        this.logger = logger;
        this.serverConsole = serverConsole;
        this.resourceProvider = resourceProvider;
        this.clientResourceService = clientResourceService;
        this.serverResourcesService = serverResourcesService;
        this.chatBox = chatBox;
    }

    private void HandleStartCommand(string name)
    {
        var serverResource = this.serverResourcesService.GetResource(name);

        if(serverResource == null)
        {
            this.logger.LogInformation("* Syntax: start <resource-name>");
            return;
        }

        this.logger.LogInformation("start: Requested by Console");

        try
        {
            if (this.serverResourcesService.StartResource(name))
            {
                this.logger.LogInformation("start: Resource '{resourceName}' started", name);
            } else
            {
                this.logger.LogWarning("start: Resource is already running");
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
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

        this.logger.LogInformation("stop: Requested by Console");
        this.logger.LogInformation("stop: Resource stopping");
        try
        {
            this.serverResourcesService.StopResource(name);
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

        this.logger.LogInformation("restart: Requested by Console");
        this.logger.LogInformation("restart: Resource restarting");
        try
        {
            if (this.serverResourcesService.StopResource(name))
            {
                this.serverResourcesService.StartResource(name);
            } else
            {
                this.logger.LogWarning("restart: Resource is not running");
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
            this.logger.LogError(ex, "Failed to restart resource {resourceName}", name);
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

    private void HandleSayCommand(string text)
    {
        this.logger.LogInformation("CONSOLECHAT: {0}", text);
        this.chatBox.Output(text, System.Drawing.Color.FromArgb(223, 149, 232));
    }

    private void HandleListCommand(string text)
    {
        int count = 0;
        int failedCount = 0;
        int runningCount = 0;
        this.logger.LogInformation("== Resource list==");
        foreach (var resource in this.resourceProvider.GetResources())
        {
            var resourceState = this.serverResourcesService.GetResourceState(resource.Name);
            switch (resourceState)
            {
                case ResourceState.Loaded:
                    count++;
                    this.logger.LogInformation(" {resourceName}    STOPPED ({resourcesFilesCount} files)", resource.Name.PadRight(20), resource.Files.Count);
                    break;
                case ResourceState.Running:
                    runningCount++;
                    this.logger.LogInformation(" {resourceName}    RUNNING", resource.Name.PadRight(20));
                    break;
            }
            this.chatBox.Output(text, System.Drawing.Color.FromArgb(223, 149, 232));
        }
        this.logger.LogInformation("== Resources: {loadedResources} loaded, {failedToStartResources} failed, {runningResources} running ==", count, failedCount, runningCount);
    }

    private void HandleHelpCommand(string text)
    {
        this.logger.LogInformation("help [command]");
        this.logger.LogInformation("Available commands:");
        Console.WriteLine();

        var commands = this.serverConsole.Commands.ToArray();
        for (int i = 0; i < commands.Length; i += 3)
        {
            string A = commands[i];
            string B = (i + 1 < commands.Length) ? commands[i + 1] : string.Empty;
            string C = (i + 2 < commands.Length) ? commands[i + 2] : string.Empty;

            Console.WriteLine(string.Format("{0,-25} {1,-25} {2,-25}", A, B, C));
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.serverConsole.AddCommand("start", HandleStartCommand);
        this.serverConsole.AddCommand("stop", HandleStopCommand);
        this.serverConsole.AddCommand("restart", HandleRestartCommand);
        this.serverConsole.AddCommand("refresh", HandleRefreshCommand);
        this.serverConsole.AddCommand("say", HandleSayCommand);
        this.serverConsole.AddCommand("list", HandleListCommand);
        this.serverConsole.AddCommand("help", HandleHelpCommand);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
