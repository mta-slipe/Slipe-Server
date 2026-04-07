using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public partial class VehicleScriptDefinitions
{
    [ScriptFunctionDefinition("createVehicle")]
    public Vehicle CreateVehicle(
        int model, float x, float y, float z,
        float rx = 0, float ry = 0, float rz = 0,
        string numberplate = "", bool direction = false,
        int variant1 = 255, int variant2 = 255, bool synced = true)
    {
        var vehicle = new Vehicle((ushort)model, new Vector3(x, y, z))
        {
            Rotation = new Vector3(rx, ry, rz),
            PlateText = numberplate,
            Variants = new VehicleVariants { Variant1 = (byte)variant1, Variant2 = (byte)variant2 },
        }.AssociateWith(server);

        if (ScriptExecutionContext.Current?.Owner != null)
            vehicle.Parent = ScriptExecutionContext.Current.Owner?.DynamicRoot;

        return vehicle;
    }

    [ScriptFunctionDefinition("blowVehicle")]
    public bool BlowVehicle(Vehicle vehicle, bool createExplosion = true)
    {
        vehicle.BlowUp(createExplosion);
        return true;
    }

    [ScriptFunctionDefinition("fixVehicle")]
    public bool FixVehicle(Vehicle vehicle)
    {
        vehicle.Fix();
        return true;
    }

    [ScriptFunctionDefinition("spawnVehicle")]
    public bool SpawnVehicle(Vehicle vehicle, float x, float y, float z, float rx = 0, float ry = 0, float rz = 0)
    {
        vehicle.Spawn(new Vector3(x, y, z), new Vector3(rx, ry, rz));
        return true;
    }

    [ScriptFunctionDefinition("respawnVehicle")]
    public bool RespawnVehicle(Vehicle vehicle)
    {
        vehicle.Respawn();
        return true;
    }

    [ScriptFunctionDefinition("isVehicleBlown")]
    public bool IsVehicleBlown(Vehicle vehicle) => vehicle.BlownState == VehicleBlownState.BlownUp;

    [ScriptFunctionDefinition("isVehicleOnGround")]
    public bool IsVehicleOnGround(Vehicle vehicle) => false;

    [ScriptFunctionDefinition("getVehiclesOfType")]
    public IEnumerable<Vehicle> GetVehiclesOfType(int model)
        => elementCollection.GetByType<Vehicle>().Where(v => v.Model == (ushort)model);

    [ScriptFunctionDefinition("getVehicleName")]
    public string GetVehicleName(Vehicle vehicle) => GetVehicleNameFromModel((int)vehicle.Model);

    [ScriptFunctionDefinition("getVehicleNameFromModel")]
    public string GetVehicleNameFromModel(int model)
        => vehicleNames.TryGetValue((ushort)model, out var name) ? name : "Unknown";

    [ScriptFunctionDefinition("getVehicleModelFromName")]
    public int GetVehicleModelFromName(string name)
        => vehicleModelsByName.TryGetValue(name.ToLowerInvariant(), out var model) ? model : -1;

    [ScriptFunctionDefinition("getVehicleType")]
    public string GetVehicleType(Vehicle vehicle) => vehicle.VehicleType switch
    {
        VehicleType.Automobile => "Automobile",
        VehicleType.Plane => "Plane",
        VehicleType.Motorcycle => "Bike",
        VehicleType.Helicopter => "Helicopter",
        VehicleType.Boat => "Boat",
        VehicleType.Train => "Train",
        VehicleType.Trailer => "Trailer",
        VehicleType.Bmx => "BMX",
        VehicleType.MonsterTruck => "Monster Truck",
        VehicleType.QuadBike => "Quad Bike",
        _ => "Unknown"
    };

    [ScriptFunctionDefinition("getVehicleMaxPassengers")]
    public int GetVehicleMaxPassengers(Vehicle vehicle) => vehicle.GetMaxPassengers();
}
