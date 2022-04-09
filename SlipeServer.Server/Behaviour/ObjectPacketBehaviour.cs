using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.Behaviour;

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
            worldObject.ScaleChanged += RelayScaleChange;
            worldObject.IsVisibleInAllDimensionsChanged += RelayIsVisibleInAllDimensionsChange;
        }
    }

    private void RelayModelChange(WorldObject sender, ElementChangedEventArgs<WorldObject, ushort> args)
    {
        this.server.BroadcastPacket(WorldObjectPacketFactory.CreateSetModelPacket(args.Source));
    }

    private void RelayScaleChange(WorldObject sender, ElementChangedEventArgs<WorldObject, Vector3> args)
    {
        this.server.BroadcastPacket(WorldObjectPacketFactory.CreateSetScalePacket(args.Source));
    }

    private void RelayIsVisibleInAllDimensionsChange(WorldObject sender, ElementChangedEventArgs<WorldObject, bool> args)
    {
        this.server.BroadcastPacket(WorldObjectPacketFactory.CreateSetVisibleInAllDimensionsPacket(args.Source));
    }
}
