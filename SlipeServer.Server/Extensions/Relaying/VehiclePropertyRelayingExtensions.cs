using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle.Sirens;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using System.Drawing;

namespace SlipeServer.Server.Extensions.Relaying;

public static class VehiclePropertyRelayingExtensions
{
    public static void AddVehicleRelayers(this Vehicle vehicle)
    {
        vehicle.ModelChanged += RelayModelChange;
        vehicle.ColorChanged += RelayColorChanged;
        vehicle.LockedStateChanged += RelayLockedStateChanged;
        vehicle.EngineStateChanged += RelayEngineStateChanged;
        vehicle.DoorStateChanged += HandleDoorStateChanged;
        vehicle.WheelStateChanged += HandleWheelStateChanged;
        vehicle.PanelStateChanged += HandlePanelStateChanged;
        vehicle.LightStateChanged += HandleLightStateChanged;
        vehicle.DoorOpenRatioChanged += HandleDoorOpenRatioChanged;
        vehicle.LandingGearChanged += RelayLandingGearChanged;
        vehicle.TaxiLightStateChanged += RelayTaxiLightStateChanged;
        vehicle.TurretRotationChanged += RelayTurretRotationChanged;
        vehicle.PlateTextChanged += RelayPlateTextChanged;
        vehicle.HeadlightColorChanged += RelayHeadlightColorChanged;
        vehicle.TowedVehicleChanged += RelayTowedVehicleChanged;
        vehicle.FuelTankExplodableChanged += RelayFuelTankExplodable;
        vehicle.UpgradeChanged += RelayUpgradeChanged;
        vehicle.PaintJobChanged += RelayPaintjobChanged;
        vehicle.VariantsChanged += RelayVariantsChanged;
        vehicle.IsDamageProofChanged += RelayIsDamageProofChanged;
        vehicle.AreDoorsDamageProofChanged += RelayAreDoorsDamageProofChanged;
        vehicle.IsDerailedChanged += RelayIsDerailedChanged;
        vehicle.IsDerailableChanged += RelayIsDerailableChanged;
        vehicle.TrainDirectionChanged += RelayTrainDirectionChanged;
        vehicle.SirensChanged += RelaySirensChanged;
        vehicle.SirenUpdated += RelaySirenUpdated;
        vehicle.AreSirensOnChanged += RelayAreSirensOn;
        vehicle.OverrideLightsChanged += RelayOverrideLights;
        vehicle.HealthChanged += RelayHealthChange;
        vehicle.Respawned += HandleRespawn;
        vehicle.PedEntered += HandleEnter;
        vehicle.PedLeft += HandleLeft;
    }

    private static void RelayOverrideLights(Vehicle sender, ElementChangedEventArgs<Vehicle, VehicleOverrideLights> args)
    {
        sender.RelayChange(new SetVehicleOverrideLightsPacket(sender.Id, (byte)args.NewValue));
    }

    private static void RelayPlateTextChanged(Element sender, ElementChangedEventArgs<Vehicle, string> args)
    {
        sender.RelayChange(VehiclePacketFactory.CreateSetPlateTextPacket(args.Source));
    }

    private static void RelayTurretRotationChanged(Element sender, ElementChangedEventArgs<Vehicle, System.Numerics.Vector2?> args)
    {
        if (args.NewValue.HasValue && !args.IsSync)
            sender.RelayChange(VehiclePacketFactory.CreateSetTurretRotationPacket(args.Source));
    }

    private static void RelayTaxiLightStateChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        sender.RelayChange(VehiclePacketFactory.CreateSetTaxiLightOnPacket(args.Source));
    }

    private static void RelayLandingGearChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        sender.RelayChange(VehiclePacketFactory.CreateSetLandingGearDownPacket(args.Source));
    }

    private static void RelayColorChanged(Vehicle sender, VehicleColorChangedEventsArgs args)
    {
        sender.RelayChange(VehiclePacketFactory.CreateSetColorPacket(args.Vehicle));
    }

    private static void RelayHeadlightColorChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, Color> args)
    {
        sender.RelayChange(VehiclePacketFactory.CreateSetHeadlightColorPacket(args.Source));
    }

    private static void RelayModelChange(Element sender, ElementChangedEventArgs<Vehicle, ushort> args)
    {
        sender.RelayChange(VehiclePacketFactory.CreateSetModelPacket(args.Source));
    }
    private static void HandleDoorStateChanged(Element sender, VehicleDoorStateChangedArgs args)
    {
        if (args.Vehicle.IsSync)
            return;
        sender.RelayChange(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Door, (byte)args.Door, (byte)args.State, args.SpawnFlyingComponent));
    }

    private static void RelayLockedStateChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        sender.RelayChange(VehiclePacketFactory.CreateSetLockedPacket(args.Source));
    }

    private static void RelayEngineStateChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        sender.RelayChange(VehiclePacketFactory.CreateSetEngineOnPacket(args.Source));
    }

    private static void HandleWheelStateChanged(Element sender, VehicleWheelStateChangedArgs args)
    {
        if (args.Vehicle.IsSync)
            return;
        sender.RelayChange(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Wheel, (byte)args.Wheel, (byte)args.State));
    }

    private static void HandlePanelStateChanged(Element sender, VehiclePanelStateChangedArgs args)
    {
        if (args.Vehicle.IsSync)
            return;
        sender.RelayChange(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Panel, (byte)args.Panel, (byte)args.State));
    }

    private static void HandleLightStateChanged(Element sender, VehicleLightStateChangedArgs args)
    {
        if (args.Vehicle.IsSync)
            return;
        sender.RelayChange(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Light, (byte)args.Light, (byte)args.State));
    }

    private static void HandleDoorOpenRatioChanged(Element sender, VehicleDoorOpenRatioChangedArgs args)
    {
        if (args.Vehicle.IsSync)
            return;
        sender.RelayChange(new SetVehicleDoorOpenRatio(args.Vehicle.Id, (byte)args.Door, args.Ratio, args.Time));
    }

    private static void RelayTowedVehicleChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, Vehicle?> args)
    {
        if (args.Source.IsSync)
            return;

        if (args.NewValue == null)
        {
            if (args.OldValue != null)
                sender.RelayChange(VehiclePacketFactory.CreateTrailerDetachPacket(sender, args.OldValue));
            return;
        }

        sender.RelayChange(new VehicleTrailerSyncPacket()
        {
            VehicleId = sender.Id,
            AttachedVehicleId = args.NewValue.Id,
            IsAttached = true,
            Position = args.NewValue.Position,
            Rotation = args.NewValue.Rotation,
            TurnVelocity = args.NewValue.TurnVelocity,
        });
    }

    private static void RelayFuelTankExplodable(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        sender.RelayChange(VehiclePacketFactory.CreateSetFuelTankExplodablePacket(args.Source));
    }

    private static void RelayUpgradeChanged(Vehicle sender, VehicleUpgradeChanged args)
    {
        if (args.NewUpgradeId.HasValue)
            sender.RelayChange(VehiclePacketFactory.CreateAddUpgradePacket(args.Vehicle, args.NewUpgradeId.Value));
        else if (args.PreviousUpgradeId.HasValue)
            sender.RelayChange(VehiclePacketFactory.CreateRemoveUpgradePacket(args.Vehicle, args.PreviousUpgradeId.Value));
    }

    private static void RelayPaintjobChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, byte> args)
    {
        sender.RelayChange(VehiclePacketFactory.CreateSetPaintjobPacket(args.Source, args.NewValue));
    }

    private static void RelayVariantsChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, ElementConcepts.VehicleVariants> args)
    {
        sender.RelayChange(new SetVehicleVariantPacket(sender.Id, args.NewValue.Variant1, args.NewValue.Variant2));
    }

    private static void RelayIsDamageProofChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        sender.RelayChange(new SetVehicleDamageProofPacket(sender.Id, args.NewValue));
    }

    private static void RelayAreDoorsDamageProofChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        sender.RelayChange(new SetVehicleDoorsDamageProofPacket(sender.Id, args.NewValue));
    }

    private static void RelayIsDerailedChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        if (args.Source.IsSync)
            return;

        sender.RelayChange(new SetTrainDerailedPacket(sender.Id, args.NewValue));
    }

    private static void RelayIsDerailableChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        sender.RelayChange(new SetTrainDerailablePacket(sender.Id, args.NewValue));
    }

    private static void RelayTrainDirectionChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, TrainDirection> args)
    {
        sender.RelayChange(new SetTrainDirectionPacket(sender.Id, args.NewValue == TrainDirection.Clockwise));
    }

    private static void RelaySirensChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, Packets.Definitions.Entities.Structs.VehicleSirenSet?> args)
    {
        if (args.NewValue == null)
            sender.RelayChange(new RemoveVehicleSirensPacket(args.Source.Id));
        else
        {
            var sirens = args.NewValue.Value;
            sender.RelayChange(new GiveVehicleSirensPacket(args.Source.Id, true, sirens.SirenType, sirens.Count, false, true, true, false));
            foreach (var siren in sirens.Sirens)
            {
                sender.RelayChange(new SetVehicleSirensPacket(
                    args.Source.Id,
                    true,
                    siren.Id,
                    siren.Position,
                    siren.Color,
                    siren.SirenMinAlpha,
                    siren.Is360,
                    siren.UsesLineOfSightCheck,
                    siren.UsesRandomizer,
                    siren.IsSilent));
            }
        }
    }

    private static void RelaySirenUpdated(Vehicle sender, VehicleSirenUpdatedEventArgs args)
    {
        var siren = args.Siren;

        sender.RelayChange(new SetVehicleSirensPacket(
            sender.Id,
            true,
            siren.Id,
            siren.Position,
            siren.Color,
            siren.SirenMinAlpha,
            siren.Is360,
            siren.UsesLineOfSightCheck,
            siren.UsesRandomizer,
            siren.IsSilent));
    }

    private static void RelayAreSirensOn(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        sender.RelayChange(new SetVehicleSirensOnPacket(sender.Id, args.NewValue));
    }

    private static void RelayHealthChange(Vehicle sender, ElementChangedEventArgs<Vehicle, float> args)
    {
        if (!args.IsSync)
            sender.RelayChange(new SetElementHealthRpcPacket(sender.Id, sender.GetAndIncrementTimeContext(), args.NewValue));
    }

    private static void HandleRespawn(Element sender, VehicleRespawnEventArgs args)
    {
        sender.RelayChange(new VehicleSpawnPacket(new VehicleSpawnInfo[] { new VehicleSpawnInfo
                {
                    ElementId = args.Vehicle.Id,
                    TimeContext = args.Vehicle.GetAndIncrementTimeContext(),
                    VehicleId = args.Vehicle.Model,
                    Position = args.Vehicle.RespawnPosition,
                    Rotation = args.Vehicle.RespawnRotation,
                    Colors = args.Vehicle.Colors.AsArray(),
                } }));
    }

    private static void HandleEnter(Element sender, VehicleEnteredEventsArgs eventArgs)
    {
        if (eventArgs.WarpsIn)
        {
            sender.RelayChange(new WarpIntoVehicleRpcPacket(
                eventArgs.Ped.Id,
                eventArgs.Vehicle.Id,
                eventArgs.Seat,
                eventArgs.Ped.GetAndIncrementTimeContext()
            ));
        }
    }

    private static void HandleLeft(Element sender, VehicleLeftEventArgs eventArgs)
    {
        if (eventArgs.WarpsOut)
        {
            sender.RelayChange(new RemoveFromVehiclePacket(
                eventArgs.Ped.Id,
                eventArgs.Ped.GetAndIncrementTimeContext()
            ));
        }
    }
}
