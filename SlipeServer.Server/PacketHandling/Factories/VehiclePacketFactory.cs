using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

namespace SlipeServer.Server.PacketHandling.Factories
{
    public static class VehiclePacketFactory
    {
        public static SetElementModelRpcPacket CreateSetModelPacket(Vehicle vehicle)
        {
            return new SetElementModelRpcPacket(vehicle.Id, vehicle.Model, vehicle.Variant1, vehicle.Variant2);
        }

        public static SetVehicleLandingGearDownRpcPacket CreateSetLandingGearDownPacket(Vehicle vehicle)
        {
            return new SetVehicleLandingGearDownRpcPacket(vehicle.Id, vehicle.IsLandingGearDown);
        }

        public static SetVehicleTaxiLightOnRpcPacket CreateSetTaxiLightOnPacket(Vehicle vehicle)
        {
            return new SetVehicleTaxiLightOnRpcPacket(vehicle.Id, vehicle.IsTaxiLightOn);
        }

        public static SetVehicleTurretRotationRpcPacket CreateSetTurretRotationPacket(Vehicle vehicle)
        {
            return new SetVehicleTurretRotationRpcPacket(vehicle.Id, vehicle.TurretRotation!.Value);
        }

        public static SetVehiclePlateTextRpcPacket CreateSetPlateTextPacket(Vehicle vehicle)
        {
            return new SetVehiclePlateTextRpcPacket(vehicle.Id, vehicle.PlateText);
        }

        public static SetVehicleColorRpcPacket CreateSetColorPacket(Vehicle vehicle)
        {
            return new SetVehicleColorRpcPacket(vehicle.Id, vehicle.Colors.AsArray());
        }
        
        public static SetVehicleHeadlightColorRpcPacket CreateSetHeadlightColorPacket(Vehicle vehicle)
        {
            return new SetVehicleHeadlightColorRpcPacket(vehicle.Id, vehicle.HeadlightColor);
        }

        public static SetVehicleLockedRpcPacket CreateSetLockedPacket(Vehicle vehicle)
        {
            return new SetVehicleLockedRpcPacket(vehicle.Id, vehicle.IsLocked);
        }

        public static SetVehicleEngineStateRpcPacket CreateSetEngineOnPacket(Vehicle vehicle)
        {
            return new SetVehicleEngineStateRpcPacket(vehicle.Id, vehicle.IsEngineOn);
        }
    }
}
