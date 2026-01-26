using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using System.Linq;
using System.Numerics;
using SlipeServer.Server.Services;
using System;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Handles ped sync, assinging ped syncers based on their distance to the ped
/// </summary>
public class PedSyncBehaviour
{
    private readonly IElementCollection elementCollection;
    private readonly Configuration configuration;

    public PedSyncBehaviour(
        IMtaServer server,
        IElementCollection elementCollection,
        Configuration configuration,
        ITimerService timerService)
    {
        this.elementCollection = elementCollection;
        this.configuration = configuration;

        timerService.CreateTimer(HandlePedSyncers, TimeSpan.FromMilliseconds(configuration.SyncIntervals.LightSync));

        server.PlayerJoined += HandlePlayerJoin;
    }

    private void HandlePlayerJoin(Player player)
    {
        player.Disconnected += HandlePlayerDisconnect;
    }

    private void HandlePlayerDisconnect(Player player, Elements.Events.PlayerQuitEventArgs e)
    {
        foreach (var ped in player.SyncingPeds.Keys.ToArray())
            StopSyncingPed(ped);

        player.Disconnected -= HandlePlayerDisconnect;
    }

    private void HandlePedSyncers()
    {
        var peds = this.elementCollection.GetByType<Ped>(ElementType.Ped)
            .Where(ped => ped is not Player);

        foreach (var ped in peds)
            UpdatePedSyncer(ped);
    }

    private void UpdatePedSyncer(Ped ped)
    {
        if (!ped.IsSyncable)
            return;

        Player? newSyncer = GetClosestPlayer(ped, this.configuration.PedSyncerDistance);

        if (newSyncer == ped.Syncer)
            return;

        if (ped.Syncer != null)
        {
            StopSyncingPed(ped);
        }

        if (newSyncer != null)
        {
            StartSyncingPed(newSyncer, ped);
        }
    }

    private void StopSyncingPed(Ped ped)
    {
        ped.Syncer?.Client.SendPacket(new PedStopSyncPacket(ped.Id));
        ped.Syncer?.SyncingPeds.TryRemove(ped, out var _);
        ped.Syncer = null;
    }

    private void StartSyncingPed(Player player, Ped ped)
    {
        player.Client.SendPacket(new PedStartSyncPacket(ped.Id, ped.Position, ped.PedRotation, ped.Velocity, ped.Health, ped.Armor));
        player.SyncingPeds.TryAdd(ped, 0);
        ped.Syncer = player;
    }

    private Player? GetClosestPlayer(Ped ped, float maxDistance)
    {
        var players = this.elementCollection
            .GetWithinRange<Player>(ped.Position, maxDistance, ElementType.Player)
            .Where(x => x.Dimension == ped.Dimension)
            .Where(x => x.Client.ConnectionState == Enums.ClientConnectionState.Joined)
            .ToArray();

        var nearestDistance = players.Any() ? players.Min(x => Vector3.Distance(x.Position, ped.Position)) : -1;
        return players.Where(x => Vector3.Distance(x.Position, ped.Position) == nearestDistance).FirstOrDefault();
    }
}
