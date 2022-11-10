using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle.Sirens;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using System.Drawing;

namespace SlipeServer.Server.Behaviour;

public class VehicleBehaviour
{
    private readonly MtaServer server;

    public VehicleBehaviour(MtaServer server)
    {
        this.server = server;

        server.ElementCreated += OnElementCreate;
    }

    private void OnElementCreate(Element element)
    {
        if (element is Vehicle vehicle)
        {
            vehicle.ModelChanged += RelayModelChange;
            vehicle.Colors.ColorChanged += RelayColorChanged;
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
            vehicle.Upgrades.UpgradeChanged += RelayUpgradeChanged;
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
        }
    }

    private void RelayOverrideLights(Vehicle sender, ElementChangedEventArgs<Vehicle, VehicleOverrideLights> args)
    {
        this.server.BroadcastPacket(new SetVehicleOverrideLightsPacket(sender.Id, (byte)args.NewValue));
    }

    private void RelayPlateTextChanged(Element sender, ElementChangedEventArgs<Vehicle, string> args)
    {
        this.server.BroadcastPacket(VehiclePacketFactory.CreateSetPlateTextPacket(args.Source));
    }

    private void RelayTurretRotationChanged(Element sender, ElementChangedEventArgs<Vehicle, System.Numerics.Vector2?> args)
    {
        if (args.NewValue.HasValue && !args.IsSync)
            this.server.BroadcastPacket(VehiclePacketFactory.CreateSetTurretRotationPacket(args.Source));
    }

    private void RelayTaxiLightStateChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        this.server.BroadcastPacket(VehiclePacketFactory.CreateSetTaxiLightOnPacket(args.Source));
    }

    private void RelayLandingGearChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        this.server.BroadcastPacket(VehiclePacketFactory.CreateSetLandingGearDownPacket(args.Source));
    }

    private void RelayColorChanged(Vehicle sender, VehicleColorChangedEventsArgs args)
    {
        this.server.BroadcastPacket(VehiclePacketFactory.CreateSetColorPacket(args.Vehicle));
    }

    private void RelayHeadlightColorChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, Color> args)
    {
        this.server.BroadcastPacket(VehiclePacketFactory.CreateSetHeadlightColorPacket(args.Source));
    }

    private void RelayModelChange(object sender, ElementChangedEventArgs<Vehicle, ushort> args)
    {
        this.server.BroadcastPacket(VehiclePacketFactory.CreateSetModelPacket(args.Source));
    }
    private void HandleDoorStateChanged(object? sender, VehicleDoorStateChangedArgs args)
    {
        if (args.Vehicle.IsSync)
            return;
        this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Door, (byte)args.Door, (byte)args.State, args.SpawnFlyingComponent));
    }

    private void RelayLockedStateChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        this.server.BroadcastPacket(VehiclePacketFactory.CreateSetLockedPacket(args.Source));
    }

    private void RelayEngineStateChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        this.server.BroadcastPacket(VehiclePacketFactory.CreateSetEngineOnPacket(args.Source));
    }

    private void HandleWheelStateChanged(object? sender, VehicleWheelStateChangedArgs args)
    {
        if (args.Vehicle.IsSync)
            return;
        this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Wheel, (byte)args.Wheel, (byte)args.State));
    }

    private void HandlePanelStateChanged(object? sender, VehiclePanelStateChangedArgs args)
    {
        if (args.Vehicle.IsSync)
            return;
        this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Panel, (byte)args.Panel, (byte)args.State));
    }

    private void HandleLightStateChanged(object? sender, VehicleLightStateChangedArgs args)
    {
        if (args.Vehicle.IsSync)
            return;
        this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Light, (byte)args.Light, (byte)args.State));
    }

    private void HandleDoorOpenRatioChanged(object? sender, VehicleDoorOpenRatioChangedArgs args)
    {
        if (args.Vehicle.IsSync)
            return;
        this.server.BroadcastPacket(new SetVehicleDoorOpenRatio(args.Vehicle.Id, (byte)args.Door, args.Ratio, args.Time));
    }

    private void RelayTowedVehicleChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, Vehicle?> args)
    {
        if (args.Source.IsSync)
            return;

        if (args.NewValue == null)
        {
            if (args.OldValue != null)
                this.server.BroadcastPacket(VehiclePacketFactory.CreateTrailerDetachPacket(sender, args.OldValue));
            return;
        }

        this.server.BroadcastPacket(new VehicleTrailerSyncPacket()
        {
            VehicleId = sender.Id,
            AttachedVehicleId = args.NewValue.Id,
            IsAttached = true,
            Position = args.NewValue.Position,
            Rotation = args.NewValue.Rotation,
            TurnVelocity = args.NewValue.TurnVelocity,
        });
    }

    private void RelayFuelTankExplodable(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        this.server.BroadcastPacket(VehiclePacketFactory.CreateSetFuelTankExplodablePacket(args.Source));
    }

    private void RelayUpgradeChanged(Vehicle sender, VehicleUpgradeChanged args)
    {
        if (args.NewUpgradeId.HasValue)
            this.server.BroadcastPacket(VehiclePacketFactory.CreateAddUpgradePacket(args.Vehicle, args.NewUpgradeId.Value));
        else if (args.PreviousUpgradeId.HasValue)
            this.server.BroadcastPacket(VehiclePacketFactory.CreateRemoveUpgradePacket(args.Vehicle, args.PreviousUpgradeId.Value));
    }

    private void RelayPaintjobChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, byte> args)
    {
        this.server.BroadcastPacket(VehiclePacketFactory.CreateSetPaintjobPacket(args.Source, args.NewValue));
    }

    private void RelayVariantsChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, ElementConcepts.VehicleVariants> args)
    {
        this.server.BroadcastPacket(new SetVehicleVariantPacket(sender.Id, args.NewValue.Variant1, args.NewValue.Variant2));
    }

    private void RelayIsDamageProofChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        this.server.BroadcastPacket(new SetVehicleDamageProofPacket(sender.Id, args.NewValue));
    }

    private void RelayAreDoorsDamageProofChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        this.server.BroadcastPacket(new SetVehicleDoorsDamageProofPacket(sender.Id, args.NewValue));
    }

    private void RelayIsDerailedChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        if (args.Source.IsSync)
            return;

        this.server.BroadcastPacket(new SetTrainDerailedPacket(sender.Id, args.NewValue));
    }

    private void RelayIsDerailableChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        this.server.BroadcastPacket(new SetTrainDerailablePacket(sender.Id, args.NewValue));
    }

    private void RelayTrainDirectionChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, TrainDirection> args)
    {
        this.server.BroadcastPacket(new SetTrainDirectionPacket(sender.Id, args.NewValue == TrainDirection.Clockwise));
    }

    private void RelaySirensChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, Packets.Definitions.Entities.Structs.VehicleSirenSet?> args)
    {
        if (args.NewValue == null)
            this.server.BroadcastPacket(new RemoveVehicleSirensPacket(args.Source.Id));
        else
        {
            var sirens = args.NewValue.Value;
            this.server.BroadcastPacket(new GiveVehicleSirensPacket(args.Source.Id, true, sirens.SirenType, sirens.Count, false, true, true, false));
            foreach (var siren in sirens.Sirens)
            {
                this.server.BroadcastPacket(new SetVehicleSirensPacket(
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

    private void RelaySirenUpdated(Vehicle sender, VehicleSirenUpdatedEventArgs args)
    {
        var siren = args.Siren;

        this.server.BroadcastPacket(new SetVehicleSirensPacket(
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

    private void RelayAreSirensOn(Vehicle sender, ElementChangedEventArgs<Vehicle, bool> args)
    {
        this.server.BroadcastPacket(new SetVehicleSirensOnPacket(sender.Id, args.NewValue));
    }

    private void RelayHealthChange(Vehicle sender, ElementChangedEventArgs<Vehicle, float> args)
    {
        if (!args.IsSync)
            this.server.BroadcastPacket(new SetElementHealthRpcPacket(sender.Id, sender.GetAndIncrementTimeContext(), args.NewValue));
    }
}
