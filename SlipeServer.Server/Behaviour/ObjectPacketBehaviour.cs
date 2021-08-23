using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.Behaviour
{
    public class ObjectPacketBehaviour
    {
        private readonly MtaServer server;

        public ObjectPacketBehaviour(MtaServer server)
        {
            this.server = server;

            server.ElementCreated += HandleElementCreate;
        }

        private void HandleElementCreate(Element obj)
        {
            if (obj is WorldObject worldObject)
            {
                worldObject.ModelChanged += RelayModelChange;
            }
        }

        private void RelayModelChange(object sender, ElementChangedEventArgs<WorldObject, ushort> args)
        {
            if (!args.IsSync)
                this.server.BroadcastPacket(WorldObjectPacketFactory.CreateSetModelPacket(args.Source));
        }
    }
}
