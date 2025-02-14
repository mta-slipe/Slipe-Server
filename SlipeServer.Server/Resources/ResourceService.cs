using Microsoft.Extensions.Logging;
using SlipeServer.Server.ElementCollections;
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

    private readonly List<Resource> startedResources = [];

    public IReadOnlyCollection<Resource> StartedResources => this.startedResources.AsReadOnly();

    public event Action<Player>? AllStarted;
    public event Action<Resource, Player>? Stopped;
    public event Action<Resource, Player>? Started;

    public ResourceService(IResourceProvider resourceProvider)
    {
        this.resourceProvider = resourceProvider;
    }

    public async Task StartResourcesForPlayer(Player player, bool parallel = true, CancellationToken cancellationToken = default)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, player.GetCancellationToken());

        cts.Token.ThrowIfCancellationRequested();

        List<Exception> exceptions = [];

        if (parallel)
        {
            var resourcesNetIds = this.startedResources.Select(x => x.NetId).ToList();
            var tcs = new TaskCompletionSource();

            void handleResourceStart(Player sender, PlayerResourceStartedEventArgs e)
            {
                if (sender == player && resourcesNetIds.Remove(e.NetId))
                {
                    try
                    {
                        this.Started?.Invoke(this.startedResources.Where(x => x.NetId == e.NetId).First(), sender);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                    finally
                    {
                        if (resourcesNetIds.Count == 0)
                            tcs.SetResult();
                    }
                }
            }

            void handlePlayerDisconnected(Player disconnectingPlayer, PlayerQuitEventArgs e)
            {
                if (player != disconnectingPlayer)
                    return;

                player.ResourceStarted -= handleResourceStart;
                player.Disconnected -= handlePlayerDisconnected;

                tcs.SetException(new Exception("Player disconnected."));
            }

            player.ResourceStarted += handleResourceStart;
            player.Disconnected += handlePlayerDisconnected;
            cts.Token.Register(() =>
            {
                tcs.TrySetException(new OperationCanceledException());
            });

            foreach (var resource in this.startedResources)
                resource.StartFor(player);

            try
            {
                await tcs.Task;
            }
            catch(Exception ex)
            {
                exceptions.Add(ex);
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
                    await resource.StartForAsync(player, cts.Token);
                    this.Started?.Invoke(resource, player);
                }
                catch (OperationCanceledException ex)
                {
                    exceptions.Add(ex);
                    break;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
        }

        if (!cts.IsCancellationRequested)
        {
            try
            {
                this.AllStarted?.Invoke(player);
            }
            catch(Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            throw new AggregateException(exceptions);
    }

    public bool AddStartResource(string name)
    {
        if (this.startedResources.Any(r => r.Name == name))
            return false;

        var resource = this.resourceProvider.GetResource(name);
        if (resource == null)
            return false;

        resource.Start();
        this.startedResources.Add(resource);

        return true;
    }
}
