using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public partial class VehicleScriptDefinitions
{
    [ScriptFunctionDefinition("getVehicleEngineState")]
    public bool GetVehicleEngineState(Vehicle vehicle) => vehicle.IsEngineOn;

    [ScriptFunctionDefinition("setVehicleEngineState")]
    public bool SetVehicleEngineState(Vehicle vehicle, bool state)
    {
        vehicle.IsEngineOn = state;
        return true;
    }

    [ScriptFunctionDefinition("isVehicleLocked")]
    public bool IsVehicleLocked(Vehicle vehicle) => vehicle.IsLocked;

    [ScriptFunctionDefinition("setVehicleLocked")]
    public bool SetVehicleLocked(Vehicle vehicle, bool locked)
    {
        vehicle.IsLocked = locked;
        return true;
    }

    [ScriptFunctionDefinition("isVehicleDamageProof")]
    public bool IsVehicleDamageProof(Vehicle vehicle) => vehicle.IsDamageProof;

    [ScriptFunctionDefinition("setVehicleDamageProof")]
    public bool SetVehicleDamageProof(Vehicle vehicle, bool damageProof)
    {
        vehicle.IsDamageProof = damageProof;
        return true;
    }

    [ScriptFunctionDefinition("isVehicleFuelTankExplodable")]
    public bool IsVehicleFuelTankExplodable(Vehicle vehicle) => vehicle.IsFuelTankExplodable;

    [ScriptFunctionDefinition("setVehicleFuelTankExplodable")]
    public bool SetVehicleFuelTankExplodable(Vehicle vehicle, bool explodable)
    {
        vehicle.IsFuelTankExplodable = explodable;
        return true;
    }

    [ScriptFunctionDefinition("setVehicleDoorsUndamageable")]
    public bool SetVehicleDoorsUndamageable(Vehicle vehicle, bool state)
    {
        vehicle.AreDoorsDamageProof = state;
        return true;
    }

    [ScriptFunctionDefinition("isVehicleNitroActivated")]
    public bool IsVehicleNitroActivated(Vehicle vehicle) => vehicle.IsNitroActivated;

    [ScriptFunctionDefinition("setVehicleNitroActivated")]
    public bool SetVehicleNitroActivated(Vehicle vehicle, bool state)
    {
        vehicle.IsNitroActivated = state;
        return true;
    }

    [ScriptFunctionDefinition("getVehicleTurretPosition")]
    public (float, float) GetVehicleTurretPosition(Vehicle vehicle)
    {
        var turret = vehicle.TurretRotation ?? Vector2.Zero;
        return (turret.X, turret.Y);
    }

    [ScriptFunctionDefinition("setVehicleTurretPosition")]
    public bool SetVehicleTurretPosition(Vehicle vehicle, float posX, float posY)
    {
        vehicle.TurretRotation = new Vector2(posX, posY);
        return true;
    }

    [ScriptFunctionDefinition("getVehicleController")]
    public Ped? GetVehicleController(Vehicle vehicle) => vehicle.Driver;

    [ScriptFunctionDefinition("getVehicleOccupant")]
    public Ped? GetVehicleOccupant(Vehicle vehicle, int seat = 0) => vehicle.GetOccupantInSeat((byte)seat);

    [ScriptFunctionDefinition("getVehicleOccupants")]
    public LuaValue GetVehicleOccupants(Vehicle vehicle)
    {
        var table = new Dictionary<LuaValue, LuaValue>();
        foreach (var kvp in vehicle.Occupants)
            table[new LuaValue(kvp.Key)] = new LuaValue(kvp.Value.Id);
        return new LuaValue(table);
    }
}
