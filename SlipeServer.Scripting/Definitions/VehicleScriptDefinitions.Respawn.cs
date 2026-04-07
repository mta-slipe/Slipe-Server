using SlipeServer.Server.Elements;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public partial class VehicleScriptDefinitions
{
    [ScriptFunctionDefinition("getVehicleRespawnDelay")]
    public uint GetVehicleRespawnDelay(Vehicle vehicle) => vehicle.RespawnDelay;

    [ScriptFunctionDefinition("setVehicleRespawnDelay")]
    public bool SetVehicleRespawnDelay(Vehicle vehicle, uint delay)
    {
        vehicle.RespawnDelay = delay;
        return true;
    }

    [ScriptFunctionDefinition("getVehicleIdleRespawnDelay")]
    public uint GetVehicleIdleRespawnDelay(Vehicle vehicle) => vehicle.IdleRespawnDelay;

    [ScriptFunctionDefinition("setVehicleIdleRespawnDelay")]
    public bool SetVehicleIdleRespawnDelay(Vehicle vehicle, uint delay)
    {
        vehicle.IdleRespawnDelay = delay;
        return true;
    }

    [ScriptFunctionDefinition("getVehicleRespawnPosition")]
    public Vector3 GetVehicleRespawnPosition(Vehicle vehicle) => vehicle.RespawnPosition;

    [ScriptFunctionDefinition("setVehicleRespawnPosition")]
    public bool SetVehicleRespawnPosition(Vehicle vehicle, float x, float y, float z)
    {
        vehicle.RespawnPosition = new Vector3(x, y, z);
        return true;
    }

    [ScriptFunctionDefinition("getVehicleRespawnRotation")]
    public Vector3 GetVehicleRespawnRotation(Vehicle vehicle) => vehicle.RespawnRotation;

    [ScriptFunctionDefinition("setVehicleRespawnRotation")]
    public bool SetVehicleRespawnRotation(Vehicle vehicle, float rx, float ry, float rz)
    {
        vehicle.RespawnRotation = new Vector3(rx, ry, rz);
        return true;
    }

    [ScriptFunctionDefinition("isVehicleRespawnable")]
    public bool IsVehicleRespawnable(Vehicle vehicle) => vehicle.IsRespawnable;

    [ScriptFunctionDefinition("toggleVehicleRespawn")]
    public bool ToggleVehicleRespawn(Vehicle vehicle, bool respawn)
    {
        vehicle.IsRespawnable = respawn;
        return true;
    }

    [ScriptFunctionDefinition("resetVehicleExplosionTime")]
    public bool ResetVehicleExplosionTime(Vehicle vehicle)
    {
        vehicle.ResetExplosionTime();
        return true;
    }

    [ScriptFunctionDefinition("resetVehicleIdleTime")]
    public bool ResetVehicleIdleTime(Vehicle vehicle)
    {
        vehicle.ResetIdleTime();
        return true;
    }
}
