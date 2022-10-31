using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using System.Collections.Generic;
using System.Drawing;
using System.Timers;

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
        }
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
        if (args.PreviousUpgradeId.HasValue)
            this.server.BroadcastPacket(VehiclePacketFactory.CreateRemoveUpgradePacket(args.Vehicle, args.PreviousUpgradeId.Value));

        if (args.NewUpgradeId.HasValue)
            this.server.BroadcastPacket(VehiclePacketFactory.CreateAddUpgradePacket(args.Vehicle, args.NewUpgradeId.Value));
    }

    private void RelayPaintjobChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, byte> args)
    {
        this.server.BroadcastPacket(VehiclePacketFactory.CreateSetPaintjobPacket(args.Source, args.NewValue));
    }
}
