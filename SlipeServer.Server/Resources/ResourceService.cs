using Microsoft.Extensions.Logging;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Resources.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SlipeServer.Server.Resources;

/// <summary>
/// Service that allows simple ways to start and stop resources (for all players)
/// </summary>
public class ResourceService
{
    private readonly IResourceProvider resourceProvider;

    private readonly List<Resource> startedResources;

    public IReadOnlyCollection<Resource> StartedResources => this.startedResources.AsReadOnly();

    public event Action<Player>? AllStarted;
    public event Action<Resource, Player>? Started;

    public ResourceService(IResourceProvider resourceProvider)
    {
        this.resourceProvider = resourceProvider;
        this.startedResources = [];
    }

    public async Task StartResourcesForPlayer(Player player, bool parallel = true, CancellationToken cancellationToken = default)
    {
        var exceptions = new List<Exception>();

        if (parallel)
        {
            var resourcesNetIds = this.startedResources.Select(x => x.NetId).ToList();
            var tcs = new TaskCompletionSource();

            void handleResourceStart(Player sender, PlayerResourceStartedEventArgs e)
            {
                if (sender == player && resourcesNetIds.Remove(e.NetId))
                {
                    Started?.Invoke(this.startedResources.Where(x => x.NetId == e.NetId).First(), sender);
                    if (resourcesNetIds.Count == 0)
                        tcs.SetResult();
                }
            }

            void handlePlayerDisconnected(Player disconnectingPlayer, PlayerQuitEventArgs e)
            {
                if (player != disconnectingPlayer)
                    return;

                player.ResourceStarted -= handleResourceStart;
                player.Disconnected -= handlePlayerDisconnected;

                tcs.SetException(new System.Exception("Player disconnected."));
            }

            player.ResourceStarted += handleResourceStart;
            player.Disconnected += handlePlayerDisconnected;
            cancellationToken.Register(() =>
            {
                tcs.TrySetException(new OperationCanceledException());
            });

            foreach (var resource in this.startedResources)
                resource.StartFor(player);

            try
            {
                await tcs.Task;
            }
            finally
            {
                player.ResourceStarted -= handleResourceStart;
                player.Disconnected -= handlePlayerDisconnected;
            }
        }
        else
        {
            foreach (var resource in this.startedResources)
            {
                try
                {
                    await resource.StartForAsync(player);
                    Started?.Invoke(resource, player);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
        }

        this.AllStarted?.Invoke(player);

        if (exceptions.Count > 0)
            throw new AggregateException(exceptions);
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
