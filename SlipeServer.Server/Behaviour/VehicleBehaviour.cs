using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using System.Collections.Generic;
using System.Timers;

namespace SlipeServer.Server.Behaviour
{
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
                vehicle.DoorStateChanged += HandleDoorStateChanged;
                vehicle.WheelStateChanged += HandleWheelStateChanged;
                vehicle.PanelStateChanged += HandlePanelStateChanged;
                vehicle.LightStateChanged += HandleLightStateChanged;
                vehicle.DoorOpenRatioChanged += HandleDoorOpenRatioChanged;
                vehicle.LandingGearChanged += RelayLandingGearChanged;
                vehicle.TaxiLightStateChanged += RelayTaxiLightStateChanged;
                vehicle.TurretRotationChanged += RelayTurretRotationChanged;
                vehicle.PlateTextChanged += RelayPlateTextChanged;
                vehicle.Upgrades.UpgradeChanged += RelayUpgradeChanged;
            }
        }

        private void RelayUpgradeChanged(Vehicle sender, VehicleUpgradeChanged args)
        {
            if(args.PreviousUpgradeId.HasValue)
                this.server.BroadcastPacket(VehiclePacketFactory.CreateRemoveUpgradePacket(args.Vehicle, args.PreviousUpgradeId.Value));

            if(args.NewUpgradeId.HasValue)
                this.server.BroadcastPacket(VehiclePacketFactory.CreateAddUpgradePacket(args.Vehicle, args.NewUpgradeId.Value));

        }

        private void RelayPlateTextChanged(Element sender, ElementChangedEventArgs<Vehicle, string> args)
        {
            this.server.BroadcastPacket(VehiclePacketFactory.CreateSetPlateTextPacket(args.Source));
        }

        private void RelayTurretRotationChanged(Element sender, ElementChangedEventArgs<Vehicle, System.Numerics.Vector2?> args)
        {
            if(args.NewValue.HasValue && !args.IsSync)
                this.server.BroadcastPacket(VehiclePacketFactory.CreateSetTurretRotationPacket(args.Source));
        }

        private void RelayTaxiLightStateChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
        {
            this.server.BroadcastPacket(VehiclePacketFactory.CreateSetVehicleTaxiLightOnPacket(args.Source));
        }

        private void RelayLandingGearChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
        {
            this.server.BroadcastPacket(VehiclePacketFactory.CreateSetLandingGearDownPacket(args.Source));
        }

        private void RelayModelChange(object sender, ElementChangedEventArgs<Vehicle, ushort> args)
        {
            this.server.BroadcastPacket(VehiclePacketFactory.CreateSetModelPacket(args.Source));
        }
        private void HandleDoorStateChanged(object? sender, VehicleDoorStateChangedArgs args)
        {
            this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Door, (byte)args.Door, (byte)args.State, args.SpawnFlyingComponent));
        }

        private void HandleWheelStateChanged(object? sender, VehicleWheelStateChangedArgs args)
        {
            this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Wheel, (byte)args.Wheel, (byte)args.State));
        }

        private void HandlePanelStateChanged(object? sender, VehiclePanelStateChangedArgs args)
        {
            this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Panel, (byte)args.Panel, (byte)args.State));
        }

        private void HandleLightStateChanged(object? sender, VehicleLightStateChangedArgs args)
        {
            this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Light, (byte)args.Light, (byte)args.State));
        }

        private void HandleDoorOpenRatioChanged(object? sender, VehicleDoorOpenRatioChangedArgs args)
        {
            this.server.BroadcastPacket(new SetVehicleDoorOpenRatio(args.Vehicle.Id, (byte)args.Door, args.Ratio, args.Time));
        }
    }
}
