using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Entities.Structs
{
    public struct VehicleHandling
    {
        public float Mass;

        public float TurnMass;
        public float DragCoefficient;
        public Vector3 CenterOfMass;
        public byte PercentSubmerged;

        public float TractionMultiplier;

        public byte DriveType;
        public byte EngineType;
        public byte NumberOfGears;

        public float EngineAcceleration;
        public float EngineInertia;
        public float MaxVelocity;

        public float BrakeDeceleration;
        public float BrakeBids;
        public bool Abs;

        public float SteeringLock;
        public float TractionLoss;
        public float TractionBias;

        public float SuspensionForceLevel;
        public float SuspensionDampening;
        public float SuspensionHighSpeedDampening;
        public float SuspennsionUpperLimit;
        public float SuspenionLowerLimit;
        public float SuspensionFrontRearBias;
        public float SuspensionAntiDiveMultiplier;

        public float CollisionDamageMultiplier;

        public uint ModelFlags;
        public uint HandlingFlags;
        public float SeatOffsetDistance;
        public byte AnimGroup;
    }
}
