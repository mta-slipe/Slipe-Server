using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;

namespace SlipeServer.Server.Extensions.Relaying;

public static class VehicleHandlingPropertyRelayingExtensions
{
    public static void AddVehicleHandlingRelayers(this Vehicle vehicle)
    {
        vehicle.HandlingChanged += RelayHandlingChange;
    }

    private static void RelayHandlingChange(Vehicle sender, ElementChangedEventArgs<Vehicle, VehicleHandling?> args)
    {
        var oldHandling = args.OldValue ?? sender.DefaultHandling;
        var newHandling = args.NewValue ?? sender.DefaultHandling;
        ApplyHandlingDiff(sender, oldHandling, newHandling);
    }
    private static void ApplyHandlingDiff(Vehicle vehicle, VehicleHandling oldHandling, VehicleHandling newHandling)
    {
        if (oldHandling.Mass != newHandling.Mass)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.Mass, newHandling.Mass));

        if (oldHandling.TurnMass != newHandling.TurnMass)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.TurnMass, newHandling.TurnMass));

        if (oldHandling.DragCoefficient != newHandling.DragCoefficient)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.DragCoefficient, newHandling.DragCoefficient));

        if (oldHandling.CenterOfMass != newHandling.CenterOfMass)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.CenterOfMass, newHandling.CenterOfMass));

        if (oldHandling.PercentSubmerged != newHandling.PercentSubmerged)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.PercentSubmerged, newHandling.PercentSubmerged));

        if (oldHandling.TractionMultiplier != newHandling.TractionMultiplier)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.TractionMultiplier, newHandling.TractionMultiplier));

        if (oldHandling.DriveType != newHandling.DriveType)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.DriveType, newHandling.DriveType.ToString().ToLower()));

        if (oldHandling.EngineType != newHandling.EngineType)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.EngineType, newHandling.EngineType.ToString().ToLower()));

        if (oldHandling.NumberOfGears != newHandling.NumberOfGears)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.NumberOfGears, newHandling.NumberOfGears));

        if (oldHandling.EngineAcceleration != newHandling.EngineAcceleration)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.EngineAcceleration, newHandling.EngineAcceleration));

        if (oldHandling.EngineInertia != newHandling.EngineInertia)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.EngineInertia, newHandling.EngineInertia));

        if (oldHandling.MaxVelocity != newHandling.MaxVelocity)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.MaxVelocity, newHandling.MaxVelocity));

        if (oldHandling.BrakeDeceleration != newHandling.BrakeDeceleration)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.BrakeDeceleration, newHandling.BrakeDeceleration));

        if (oldHandling.BrakeBias != newHandling.BrakeBias)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.BrakeBias, newHandling.BrakeBias));

        if (oldHandling.Abs != newHandling.Abs)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.Abs, newHandling.Abs));

        if (oldHandling.SteeringLock != newHandling.SteeringLock)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SteeringLock, newHandling.SteeringLock));

        if (oldHandling.TractionLoss != newHandling.TractionLoss)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.TractionLoss, newHandling.TractionLoss));

        if (oldHandling.TractionBias != newHandling.TractionBias)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.TractionBias, newHandling.TractionBias));

        if (oldHandling.SuspensionForceLevel != newHandling.SuspensionForceLevel)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionForceLevel, newHandling.SuspensionForceLevel));

        if (oldHandling.SuspensionDampening != newHandling.SuspensionDampening)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionDamping, newHandling.SuspensionDampening));

        if (oldHandling.SuspensionHighSpeedDampening != newHandling.SuspensionHighSpeedDampening)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionHighSpeedDaming, newHandling.SuspensionHighSpeedDampening));

        if (oldHandling.SuspensionUpperLimit != newHandling.SuspensionUpperLimit)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionUpperLimit, newHandling.SuspensionUpperLimit));

        if (oldHandling.SuspensionLowerLimit != newHandling.SuspensionLowerLimit)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionLowerLimit, newHandling.SuspensionLowerLimit));

        if (oldHandling.SuspensionFrontRearBias != newHandling.SuspensionFrontRearBias)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionFrontRearBias, newHandling.SuspensionFrontRearBias));

        if (oldHandling.SuspensionAntiDiveMultiplier != newHandling.SuspensionAntiDiveMultiplier)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionAntiDiveMultiplier, newHandling.SuspensionAntiDiveMultiplier));

        if (oldHandling.CollisionDamageMultiplier != newHandling.CollisionDamageMultiplier)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.CollisionDamageMultiplier, newHandling.CollisionDamageMultiplier));

        if (oldHandling.ModelFlags != newHandling.ModelFlags)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.ModelFlags, newHandling.ModelFlags));

        if (oldHandling.HandlingFlags != newHandling.HandlingFlags)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.HandlingFlags, newHandling.HandlingFlags));

        if (oldHandling.SeatOffsetDistance != newHandling.SeatOffsetDistance)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SeatOffsetDistance, newHandling.SeatOffsetDistance));

        if (oldHandling.AnimGroup != newHandling.AnimGroup)
            vehicle.RelayChange(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.AnimGroup, newHandling.AnimGroup));
    }
}
