using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;

namespace SlipeServer.Scripting.Definitions;

public partial class VehicleScriptDefinitions
{
    [ScriptFunctionDefinition("getTrainDirection")]
    public bool GetTrainDirection(Vehicle vehicle) => vehicle.TrainDirection == TrainDirection.Clockwise;

    [ScriptFunctionDefinition("setTrainDirection")]
    public bool SetTrainDirection(Vehicle vehicle, bool clockwise)
    {
        vehicle.TrainDirection = clockwise ? TrainDirection.Clockwise : TrainDirection.CounterClockwise;
        return true;
    }

    [ScriptFunctionDefinition("getTrainPosition")]
    public float GetTrainPosition(Vehicle vehicle) => vehicle.TrainPosition;

    [ScriptFunctionDefinition("setTrainPosition")]
    public bool SetTrainPosition(Vehicle vehicle, float position)
    {
        vehicle.TrainPosition = position;
        return true;
    }

    [ScriptFunctionDefinition("getTrainSpeed")]
    public float GetTrainSpeed(Vehicle vehicle) => vehicle.TrainSpeed;

    [ScriptFunctionDefinition("setTrainSpeed")]
    public bool SetTrainSpeed(Vehicle vehicle, float speed)
    {
        vehicle.TrainSpeed = speed;
        return true;
    }

    [ScriptFunctionDefinition("isTrainDerailable")]
    public bool IsTrainDerailable(Vehicle vehicle) => vehicle.IsDerailable;

    [ScriptFunctionDefinition("setTrainDerailable")]
    public bool SetTrainDerailable(Vehicle vehicle, bool derailable)
    {
        vehicle.IsDerailable = derailable;
        return true;
    }

    [ScriptFunctionDefinition("isTrainDerailed")]
    public bool IsTrainDerailed(Vehicle vehicle) => vehicle.IsDerailed;

    [ScriptFunctionDefinition("setTrainDerailed")]
    public bool SetTrainDerailed(Vehicle vehicle, bool derailed)
    {
        vehicle.IsDerailed = derailed;
        return true;
    }
}
