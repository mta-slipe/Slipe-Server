using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Repositories;
using System.Linq;
using System.Numerics;
using System.Timers;

namespace SlipeServer.Server.Behaviour;

public class UnoccupiedVehicleSyncBehaviour
{
    private readonly IElementRepository elementRepository;
    private readonly Configuration configuration;
    private readonly Timer timer;

    public UnoccupiedVehicleSyncBehaviour(
        MtaServer server,
        IElementRepository elementRepository,
        Configuration configuration)
    {
        this.elementRepository = elementRepository;
        this.configuration = configuration;

        this.timer = new Timer(configuration.SyncIntervals.LightSync)
        {
            AutoReset = true,
        };
        this.timer.Start();
        this.timer.Elapsed += (sender, args) => HandleVehicleSyncers();

        server.PlayerJoined += HandlePlayerJoin;
        server.ElementCreated += HandleElementCreation;
    }

    private void HandlePlayerJoin(Player player)
    {
        player.Disconnected += HandlePlayerDisconnect;
    }

    private void HandleElementCreation(Element element)
    {
        if (element is Vehicle vehicle)
        {
            vehicle.Pushed += HandleVehiclePush;
            vehicle.Destroyed += (e) => vehicle.Pushed -= HandleVehiclePush;
        }
    }

    private void HandleVehiclePush(Vehicle vehicle, Elements.Events.VehiclePushedEventArgs e)
    {
        StopSyncingVehicle(vehicle);
        StartSyncingVehicle(e.Pusher, vehicle);
    }

    private void HandlePlayerDisconnect(Player player, Elements.Events.PlayerQuitEventArgs e)
    {
        foreach (var vehicle in player.SyncingVehicles.ToArray())
            StopSyncingVehicle(vehicle);

        player.Disconnected -= HandlePlayerDisconnect;
    }

    private void HandleVehicleSyncers()
    {
        var vehicles = this.elementRepository.GetByType<Vehicle>(ElementType.Vehicle);

        foreach (var vehicle in vehicles)
            UpdateVehicleSyncer(vehicle);
    }

    private void UpdateVehicleSyncer(Vehicle vehicle)
    {
        Player? newSyncer = GetClosestPlayer(vehicle, this.configuration.UnoccupiedVehicleSyncerDistance);

        if (newSyncer == vehicle.Syncer)
            return;

        if (vehicle.Syncer != null)
        {
            StopSyncingVehicle(vehicle);
        }

        if (newSyncer != null)
        {
            StartSyncingVehicle(newSyncer, vehicle);
        }
    }

    private void StopSyncingVehicle(Vehicle vehicle)
    {
        vehicle.Syncer?.Client.SendPacket(new UnoccupiedVehicleSyncStopPacket(vehicle.Id));
        vehicle.Syncer?.SyncingVehicles.Remove(vehicle);
        vehicle.Syncer = null;
    }

    private void StartSyncingVehicle(Player player, Vehicle vehicle)
    {
        player.Client.SendPacket(new UnoccupiedVehicleSyncStartPacket()
        {
            ElementId = vehicle.Id,
            Position = vehicle.Position,
            Rotation = vehicle.Rotation,
            Velocity = vehicle.Velocity,
            TurnVelocity = vehicle.TurnVelocity,
            Health = vehicle.Health,
        });
        player.SyncingVehicles.Add(vehicle);
        vehicle.Syncer = player;
    }

    private Player? GetClosestPlayer(Vehicle vehicle, float maxDistance)
    {
        var players = this.elementRepository
            .GetWithinRange<Player>(vehicle.Position, maxDistance, ElementType.Player)
            .Where(x => x.Dimension == vehicle.Dimension)
            .ToArray();

        var nearestDistance = players.Any() ? players.Min(x => Vector3.Distance(x.Position, vehicle.Position)) : -1;
        return players.Where(x => Vector3.Distance(x.Position, vehicle.Position) == nearestDistance).FirstOrDefault();
    }
}
