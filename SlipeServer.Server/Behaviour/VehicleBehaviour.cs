using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
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
                vehicle.Colors.ColorChanged += RelayColorChanged;
                vehicle.LockedStateChanged += RelayLockedStateChanged;
                vehicle.EngineStateChanged += RelayEngineStateChanged; ;
            }
        }

        private void RelayColorChanged(Vehicle sender, VehicleColorChangedEventsArgs args)
        {
            this.server.BroadcastPacket(VehiclePacketFactory.CreateSetColorPacket(args.Vehicle));
        }

        private void RelayModelChange(object sender, ElementChangedEventArgs<Vehicle, ushort> args)
        {
            this.server.BroadcastPacket(VehiclePacketFactory.CreateSetModelPacket(args.Source));
        }

        private void RelayLockedStateChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
        {
            this.server.BroadcastPacket(VehiclePacketFactory.CreateSetLockedPacket(args.Source));
        }

        private void RelayEngineStateChanged(Element sender, ElementChangedEventArgs<Vehicle, bool> args)
        {
            this.server.BroadcastPacket(VehiclePacketFactory.CreateSetLockedPacket(args.Source));
        }
    }
}
