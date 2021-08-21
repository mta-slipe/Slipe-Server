using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
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
                vehicle.DamageStateChanged += HandleDoorStateChanged;
            }
        }

        private void HandleDoorStateChanged(object? sender, VehicleDamageStateChanged args)
        {
            this.server.BroadcastPacket(new SetVehicleDamageState(args.Vehicle.Id, (byte)args.Part, args.Door, args.State, args.SpawnFlyingComponent));
        }
    }
}
