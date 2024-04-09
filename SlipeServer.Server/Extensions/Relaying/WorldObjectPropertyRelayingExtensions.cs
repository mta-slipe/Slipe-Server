using SlipeServer.Packets.Definitions.Lua.ElementRpc.WorldObject;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using System.Numerics;

namespace SlipeServer.Server.Extensions.Relaying;

public static class WorldObjectPropertyRelayingExtensions
{
    public static void AddWorldObjectRelayers(this WorldObject worldObject)
    {
        worldObject.ModelChanged += RelayModelChange;
        worldObject.ScaleChanged += RelayScaleChange;
        worldObject.IsVisibleInAllDimensionsChanged += RelayIsVisibleInAllDimensionsChange;
        worldObject.Moved += RelayMovement;
        worldObject.MovementCancelled += RelayMovementCancel;
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

    private static void RelayMovement(WorldObject sender, WorldObjectMovedEventArgs e)
    {
        sender.RelayChange(new MoveObjectRpcPacket(sender.Id, e.Movement));
    }

    private static void RelayMovementCancel(WorldObject sender, WorldObjectMovementCancelledEventArgs e)
    {
        sender.RelayChange(new StopObjectRpcPacket(sender.Id, e.NewPosition, e.NewRotation));
    }
}
