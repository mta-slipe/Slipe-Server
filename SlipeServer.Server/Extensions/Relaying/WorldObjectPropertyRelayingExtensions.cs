using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Marker;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Extensions.Relaying;

public static class WorldObjectPropertyRelayingExtensions
{
    public static void AddWorldObjectRelayers(this WorldObject worldObject)
    {
        worldObject.ModelChanged += RelayModelChange;
        worldObject.ScaleChanged += RelayScaleChange;
        worldObject.IsVisibleInAllDimensionsChanged += RelayIsVisibleInAllDimensionsChange;
    }

    private static void RelayModelChange(WorldObject sender, ElementChangedEventArgs<WorldObject, ushort> args)
    {
        sender.RelayChange(WorldObjectPacketFactory.CreateSetModelPacket(args.Source));
    }

    private static void RelayScaleChange(WorldObject sender, ElementChangedEventArgs<WorldObject, Vector3> args)
    {
        sender.RelayChange(WorldObjectPacketFactory.CreateSetScalePacket(args.Source));
    }

    private static void RelayIsVisibleInAllDimensionsChange(WorldObject sender, ElementChangedEventArgs<WorldObject, bool> args)
    {
        sender.RelayChange(WorldObjectPacketFactory.CreateSetVisibleInAllDimensionsPacket(args.Source));
    }
}
