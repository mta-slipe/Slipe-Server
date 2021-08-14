using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Entities.Structs
{
    public struct VehicleHandling
    {
        public float Mass { get; set; }

        public float TurnMass { get; set; }
        public float DragCoefficient { get; set; }
        public Vector3 CenterOfMass { get; set; }
        public byte PercentSubmerged { get; set; }

        public float TractionMultiplier { get; set; }

        public byte DriveType { get; set; }
        public byte EngineType { get; set; }
        public byte NumberOfGears { get; set; }

        public float EngineAcceleration { get; set; }
        public float EngineInertia { get; set; }
        public float MaxVelocity { get; set; }

        public float BrakeDeceleration { get; set; }
        public float BrakeBids { get; set; }
        public bool Abs { get; set; }

        public float SteeringLock { get; set; }
        public float TractionLoss { get; set; }
        public float TractionBias { get; set; }

        public float SuspensionForceLevel { get; set; }
        public float SuspensionDampening { get; set; }
        public float SuspensionHighSpeedDampening { get; set; }
        public float SuspennsionUpperLimit { get; set; }
        public float SuspenionLowerLimit { get; set; }
        public float SuspensionFrontRearBias { get; set; }
        public float SuspensionAntiDiveMultiplier { get; set; }

        public float CollisionDamageMultiplier { get; set; }

        public uint ModelFlags { get; set; }
        public uint HandlingFlags { get; set; }
        public float SeatOffsetDistance { get; set; }
        public byte AnimGroup { get; set; }
    }
}
