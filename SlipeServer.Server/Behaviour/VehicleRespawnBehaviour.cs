using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System;
using System.Timers;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Periodically checks vehicles and respawns them based on their respawn settings.
/// Blown vehicles are respawned after their RespawnDelay has elapsed.
/// Idle vehicles (no occupants, not moved recently) are respawned after their IdleRespawnDelay has elapsed.
/// Only vehicles with IsRespawnable = true and no occupants are considered.
/// </summary>
public class VehicleRespawnBehaviour
{
    private readonly IElementCollection elementCollection;
    private readonly Timer timer;

    public VehicleRespawnBehaviour(IMtaServer server, IElementCollection elementCollection)
    {
        this.elementCollection = elementCollection;

        this.timer = new Timer(1000) { AutoReset = true };
        this.timer.Elapsed += (_, _) => CheckVehicles();
        this.timer.Start();
    }

    private void CheckVehicles()
    {
        var now = DateTime.UtcNow;

        foreach (var vehicle in this.elementCollection.GetByType<Vehicle>(ElementType.Vehicle))
        {
            if (!vehicle.IsRespawnable)
                continue;

            if (vehicle.Occupants.Count > 0)
                continue;

            if (vehicle.BlownAtUtc.HasValue)
            {
                var elapsed = now - vehicle.BlownAtUtc.Value;
                if (elapsed.TotalMilliseconds >= vehicle.RespawnDelay)
                    vehicle.Respawn();
            }
            else if ((now - vehicle.LastMovedAtUtc).TotalMilliseconds >= vehicle.IdleRespawnDelay)
            {
                vehicle.Respawn();
            }
        }
    }
}
