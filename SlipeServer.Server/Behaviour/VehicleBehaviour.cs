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
            }
        }

        private void RelayModelChange(object sender, ElementChangedEventArgs<Vehicle, ushort> args)
        {
            this.server.BroadcastPacket(VehiclePacketFactory.CreateSetModelPacket(args.Source));
        }

    }
}
