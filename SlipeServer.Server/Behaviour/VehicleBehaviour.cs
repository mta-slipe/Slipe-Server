using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
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
                vehicle.DoorStateChanged += HandleDoorStateChanged;
                vehicle.WheelStateChanged += HandleWheelStateChanged;
                vehicle.PanelStateChanged += HandlePanelStateChanged;
            }
        }

        private void HandleDoorStateChanged(object? sender, VehicleDoorStateChangedArgs args)
        {
            this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Door, (byte)args.Door, (byte)args.State, args.SpawnFlyingComponent));
        }

        private void HandleWheelStateChanged(object? sender, VehicleWheelStateChangedArgs args)
        {
            this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Door, (byte)args.Wheel, (byte)args.State));
        }

        private void HandlePanelStateChanged(object? sender, VehiclePanelStateChangedArgs args)
        {
            this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)VehicleDamagePart.Panel, (byte)args.Panel, (byte)args.State));
        }
    }
}
