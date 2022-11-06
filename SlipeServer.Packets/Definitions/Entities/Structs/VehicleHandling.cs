using SlipeServer.Packets.Enums;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Entities.Structs;

public struct VehicleHandling
{
    public float Mass { get; set; }

    public float TurnMass { get; set; }
    public float DragCoefficient { get; set; }
    public Vector3 CenterOfMass { get; set; }
    public byte PercentSubmerged { get; set; }

    public float TractionMultiplier { get; set; }

    public VehicleDriveType DriveType { get; set; }
    public VehicleEngineType EngineType { get; set; }
    public byte NumberOfGears { get; set; }

    public float EngineAcceleration { get; set; }
    public float EngineInertia { get; set; }
    public float MaxVelocity { get; set; }

    public float BrakeDeceleration { get; set; }
    public float BrakeBias { get; set; }
    public bool Abs { get; set; }

    public float SteeringLock { get; set; }
    public float TractionLoss { get; set; }
    public float TractionBias { get; set; }

    public float SuspensionForceLevel { get; set; }
    public float SuspensionDampening { get; set; }
    public float SuspensionHighSpeedDampening { get; set; }
    public float SuspensionUpperLimit { get; set; }
    public float SuspensionLowerLimit { get; set; }
    public float SuspensionFrontRearBias { get; set; }
    public float SuspensionAntiDiveMultiplier { get; set; }

    public float CollisionDamageMultiplier { get; set; }

    public uint ModelFlags { get; set; }
    public uint HandlingFlags { get; set; }
    public float SeatOffsetDistance { get; set; }
    public byte AnimGroup { get; set; }
}
