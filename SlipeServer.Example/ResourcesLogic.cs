using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Services;

namespace SlipeServer.Example;

public class ResourcesLogic
{
    private readonly ChatBox chatBox;
    private readonly ResourceService resourceService;
    private readonly ILogger logger;

    public ResourcesLogic(MtaServer mtaServer, ChatBox chatBox, IResourceProvider resourceProvider, ResourceService resourceService, ILogger logger)
    {
        this.chatBox = chatBox;
        this.resourceService = resourceService;
        this.logger = logger;
        this.resourceService.AddStartResource("ResourceA");
        this.resourceService.AddStartResource("ResourceB");
        this.resourceService.AllStarted += HandleAllStarted;
        mtaServer.PlayerJoined += HandlePlayerJoin;
    }

    private async void HandlePlayerJoin(Player player)
    {
        try
        {
            await this.resourceService.StartResourcesForPlayer(player);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to start resources for {playerName}", player.Name);
        }
    }

    private void HandleAllStarted(Player player)
    {
        Console.WriteLine("All resources started for: {0}", player.Name);
        this.chatBox.Output($"All resources started for: {player.Name}");
    }
}
