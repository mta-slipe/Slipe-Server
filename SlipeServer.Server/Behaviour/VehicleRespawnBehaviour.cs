using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;

namespace SlipeServer.Server.Behaviour;

public class VehicleRespawnBehaviour
{
    private readonly MtaServer server;

    public VehicleRespawnBehaviour(MtaServer server)
    {
        this.server = server;

        server.ElementCreated += OnElementCreate;
    }

    private void OnElementCreate(Element element)
    {
        if (element is Vehicle vehicle)
        {
            vehicle.Respawned += HandleRespawn;
        }
    }

    private void HandleRespawn(object? sender, VehicleRespawnEventArgs args)
    {
        this.server.BroadcastPacket(new VehicleSpawnPacket(new VehicleSpawnInfo[] { new VehicleSpawnInfo
                {
                    ElementId = args.Vehicle.Id,
                    TimeContext = args.Vehicle.GetAndIncrementTimeContext(),
                    VehicleId = args.Vehicle.Model,
                    Position = args.Vehicle.RespawnPosition,
                    Rotation = args.Vehicle.RespawnRotation,
                    Colors = args.Vehicle.Colors.AsArray(),
                } }));
    }
}
