using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;

namespace SlipeServer.Server.Behaviour;

public class VehicleHandlingBehaviour
{
    private readonly MtaServer server;

    /// <summary>
    /// Handles the relaying of vehicle handling changes
    /// </summary>
    /// <param name="server"></param>
    public VehicleHandlingBehaviour(MtaServer server)
    {
        this.server = server;

        server.ElementCreated += OnElementCreate;
    }

    private void OnElementCreate(Element element)
    {
        if (element is Vehicle vehicle)
        {
            vehicle.HandlingChanged += RelayHandlingChange;
        }
    }

    private void RelayHandlingChange(Vehicle sender, ElementChangedEventArgs<Vehicle, VehicleHandling?> args)
    {
        var oldHandling = args.OldValue ?? sender.DefaultHandling;
        var newHandling = args.NewValue ?? sender.DefaultHandling;
        ApplyHandlingDiff(sender, oldHandling, newHandling);
    }

    private void ApplyHandlingDiff(Vehicle vehicle, VehicleHandling oldHandling, VehicleHandling newHandling)
    {
        if (oldHandling.Mass != newHandling.Mass)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.Mass, newHandling.Mass));

        if (oldHandling.TurnMass != newHandling.TurnMass)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.TurnMass, newHandling.TurnMass));

        if (oldHandling.DragCoefficient != newHandling.DragCoefficient)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.DragCoefficient, newHandling.DragCoefficient));

        if (oldHandling.CenterOfMass != newHandling.CenterOfMass)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.CenterOfMass, newHandling.CenterOfMass));

        if (oldHandling.PercentSubmerged != newHandling.PercentSubmerged)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.PercentSubmerged, newHandling.PercentSubmerged));

        if (oldHandling.TractionMultiplier != newHandling.TractionMultiplier)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.TractionMultiplier, newHandling.TractionMultiplier));

        if (oldHandling.DriveType != newHandling.DriveType)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.DriveType, newHandling.DriveType.ToString().ToLower()));

        if (oldHandling.EngineType != newHandling.EngineType)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.EngineType, newHandling.EngineType.ToString().ToLower()));

        if (oldHandling.NumberOfGears != newHandling.NumberOfGears)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.NumberOfGears, newHandling.NumberOfGears));

        if (oldHandling.EngineAcceleration != newHandling.EngineAcceleration)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.EngineAcceleration, newHandling.EngineAcceleration));

        if (oldHandling.EngineInertia != newHandling.EngineInertia)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.EngineInertia, newHandling.EngineInertia));

        if (oldHandling.MaxVelocity != newHandling.MaxVelocity)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.MaxVelocity, newHandling.MaxVelocity));

        if (oldHandling.BrakeDeceleration != newHandling.BrakeDeceleration)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.BrakeDeceleration, newHandling.BrakeDeceleration));

        if (oldHandling.BrakeBias != newHandling.BrakeBias)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.BrakeBias, newHandling.BrakeBias));

        if (oldHandling.Abs != newHandling.Abs)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.Abs, newHandling.Abs));

        if (oldHandling.SteeringLock != newHandling.SteeringLock)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SteeringLock, newHandling.SteeringLock));

        if (oldHandling.TractionLoss != newHandling.TractionLoss)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.TractionLoss, newHandling.TractionLoss));

        if (oldHandling.TractionBias != newHandling.TractionBias)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.TractionBias, newHandling.TractionBias));

        if (oldHandling.SuspensionForceLevel != newHandling.SuspensionForceLevel)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionForceLevel, newHandling.SuspensionForceLevel));

        if (oldHandling.SuspensionDampening != newHandling.SuspensionDampening)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionDamping, newHandling.SuspensionDampening));

        if (oldHandling.SuspensionHighSpeedDampening != newHandling.SuspensionHighSpeedDampening)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionHighSpeedDaming, newHandling.SuspensionHighSpeedDampening));

        if (oldHandling.SuspensionUpperLimit != newHandling.SuspensionUpperLimit)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionUpperLimit, newHandling.SuspensionUpperLimit));

        if (oldHandling.SuspensionLowerLimit != newHandling.SuspensionLowerLimit)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionLowerLimit, newHandling.SuspensionLowerLimit));

        if (oldHandling.SuspensionFrontRearBias != newHandling.SuspensionFrontRearBias)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionFrontRearBias, newHandling.SuspensionFrontRearBias));

        if (oldHandling.SuspensionAntiDiveMultiplier != newHandling.SuspensionAntiDiveMultiplier)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SuspensionAntiDiveMultiplier, newHandling.SuspensionAntiDiveMultiplier));

        if (oldHandling.CollisionDamageMultiplier != newHandling.CollisionDamageMultiplier)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.CollisionDamageMultiplier, newHandling.CollisionDamageMultiplier));

        if (oldHandling.ModelFlags != newHandling.ModelFlags)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.ModelFlags, newHandling.ModelFlags));

        if (oldHandling.HandlingFlags != newHandling.HandlingFlags)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.HandlingFlags, newHandling.HandlingFlags));

        if (oldHandling.SeatOffsetDistance != newHandling.SeatOffsetDistance)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.SeatOffsetDistance, newHandling.SeatOffsetDistance));

        if (oldHandling.AnimGroup != newHandling.AnimGroup)
            this.server.BroadcastPacket(new SetVehicleHandlingPropertyRpcPacket(vehicle.Id, (byte)VehicleHandlingProperty.AnimGroup, newHandling.AnimGroup));
    }
}
