using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;

namespace SlipeServer.Scripting.Definitions;

public partial class VehicleScriptDefinitions
{
    [ScriptFunctionDefinition("getVehicleTowedByVehicle")]
    public Vehicle? GetVehicleTowedByVehicle(Vehicle vehicle) => vehicle.TowedVehicle;

    [ScriptFunctionDefinition("getVehicleTowingVehicle")]
    public Vehicle? GetVehicleTowingVehicle(Vehicle vehicle) => vehicle.TowingVehicle;

    [ScriptFunctionDefinition("attachTrailerToVehicle")]
    public bool AttachTrailerToVehicle(Vehicle vehicle, Vehicle trailer)
    {
        vehicle.AttachTrailer(trailer);
        return true;
    }

    [ScriptFunctionDefinition("detachTrailerFromVehicle")]
    public bool DetachTrailerFromVehicle(Vehicle vehicle, Vehicle? trailer = null)
    {
        if (trailer != null)
            vehicle.AttachTrailer(null);
        else
            vehicle.DetachTrailer();
        return true;
    }

    [ScriptFunctionDefinition("getVehicleDoorState")]
    public int GetVehicleDoorState(Vehicle vehicle, int door)
        => (int)vehicle.GetDoorState((VehicleDoor)door);

    [ScriptFunctionDefinition("setVehicleDoorState")]
    public bool SetVehicleDoorState(Vehicle vehicle, int door, int state, bool spawnFlyingComponent = false)
    {
        vehicle.SetDoorState((VehicleDoor)door, (VehicleDoorState)state, spawnFlyingComponent);
        return true;
    }

    [ScriptFunctionDefinition("getVehicleWheelStates")]
    public (int, int, int, int) GetVehicleWheelStates(Vehicle vehicle)
        => ((int)vehicle.GetWheelState(VehicleWheel.FrontLeft),
            (int)vehicle.GetWheelState(VehicleWheel.RearLeft),
            (int)vehicle.GetWheelState(VehicleWheel.FrontRight),
            (int)vehicle.GetWheelState(VehicleWheel.RearRight));

    [ScriptFunctionDefinition("setVehicleWheelStates")]
    public bool SetVehicleWheelStates(Vehicle vehicle, int frontLeft, int rearLeft = -1, int frontRight = -1, int rearRight = -1)
    {
        if (frontLeft >= 0) vehicle.SetWheelState(VehicleWheel.FrontLeft, (VehicleWheelState)frontLeft);
        if (rearLeft >= 0) vehicle.SetWheelState(VehicleWheel.RearLeft, (VehicleWheelState)rearLeft);
        if (frontRight >= 0) vehicle.SetWheelState(VehicleWheel.FrontRight, (VehicleWheelState)frontRight);
        if (rearRight >= 0) vehicle.SetWheelState(VehicleWheel.RearRight, (VehicleWheelState)rearRight);
        return true;
    }

    [ScriptFunctionDefinition("getVehiclePanelState")]
    public int GetVehiclePanelState(Vehicle vehicle, int panel)
        => (int)vehicle.GetPanelState((VehiclePanel)panel);

    [ScriptFunctionDefinition("setVehiclePanelState")]
    public bool SetVehiclePanelState(Vehicle vehicle, int panel, int state)
    {
        vehicle.SetPanelState((VehiclePanel)panel, (VehiclePanelState)state);
        return true;
    }

    [ScriptFunctionDefinition("getVehicleLightState")]
    public int GetVehicleLightState(Vehicle vehicle, int light)
        => (int)vehicle.GetLightState((VehicleLight)light);

    [ScriptFunctionDefinition("setVehicleLightState")]
    public bool SetVehicleLightState(Vehicle vehicle, int light, int state)
    {
        vehicle.SetLightState((VehicleLight)light, (VehicleLightState)state);
        return true;
    }

    [ScriptFunctionDefinition("getVehicleDoorOpenRatio")]
    public float GetVehicleDoorOpenRatio(Vehicle vehicle, int door)
        => vehicle.GetDoorOpenRatio((VehicleDoor)door);

    [ScriptFunctionDefinition("setVehicleDoorOpenRatio")]
    public bool SetVehicleDoorOpenRatio(Vehicle vehicle, int door, float ratio, uint time = 0)
    {
        vehicle.SetDoorOpenRatio((VehicleDoor)door, ratio, time);
        return true;
    }

    [ScriptFunctionDefinition("spawnVehicleFlyingComponent")]
    public bool SpawnVehicleFlyingComponent(Vehicle vehicle, int door, int vehicleDoorState = 0) => true;
}
