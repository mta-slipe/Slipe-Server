using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using System.Numerics;

namespace SlipeServer.Server.Extensions.Relaying;

public static class ElementPropertyRelayingExtensions
{
    public static void AddRelayers(this Element element)
    {
        AddElementRelayers(element);

        switch (element)
        {
            case Blip blip:
                blip.AddBlipRelayers();
                break;
            case CollisionShape collisionShape:
                collisionShape.AddCollisionShapeRelayers();
                break;
            case Marker marker:
                marker.AddMarkerRelayers();
                break;
            case Player player:
                player.AddPedRelayers();
                player.AddPlayerRelayers();
                break;
            case Ped ped:
                ped.AddPedRelayers();
                break;
            case RadarArea radarArea:
                radarArea.AddRadarAreaRelayers();
                break;
            case Vehicle vehicle:
                vehicle.AddVehicleRelayers();
                vehicle.AddVehicleHandlingRelayers();
                break;

        }
    }

    private static void AddElementRelayers(Element element)
    {
        element.AssociatedWith += RelayAssociation;
        element.RemovedFrom += RelayRemoval;
        element.PositionChanged += RelayPositionChange;
        element.RotationChanged += RelayRotationChange;
        element.AlphaChanged += RelayAlphaChange;
        element.DimensionChanged += RelayDimensionChange;
        element.InteriorChanged += RelayInteriorChange;
        element.CallPropagationChanged += RelayCallPropagationChanged;
        element.CollisionEnabledhanged += RelayCollisionEnabledhanged;
        element.FrozenChanged += RelayElementFrozenChanged;
        element.Destroyed += RelayElementDestroy;
        element.Attached += RelayAttached;
        element.Detached += RelayDetached;
        element.AttachedOffsetChanged += RelayAttachedOffsetChanged;
    }

    private static void RelayAssociation(Element sender, ElementAssociatedWithEventArgs e) =>
        sender.CreateFor(sender.AssociatedPlayers);

    private static void RelayRemoval(Element sender, ElementAssociatedWithEventArgs e) =>
        RelayElementDestroy(sender);

    private static void RelayPositionChange(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        if (!args.IsSync) 
            RelayChange(sender, ElementPacketFactory.CreateSetPositionPacket(sender, args.NewValue));
    }

    private static void RelayRotationChange(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        if (sender is not Player && sender is not Blip)
            if (!args.IsSync)
                RelayChange(sender, ElementPacketFactory.CreateSetRotationPacket(args.Source, args.NewValue));
    }
    private static void RelayAlphaChange(Element sender, ElementChangedEventArgs<byte> args)
    {
        if (!args.IsSync) 
            RelayChange(sender, ElementPacketFactory.CreateSetAlphaPacket(args.Source, args.NewValue));
    }

    private static void RelayDimensionChange(Element sender, ElementChangedEventArgs<ushort> args)
    {
        if (!args.IsSync) 
            RelayChange(sender, ElementPacketFactory.CreateSetDimensionPacket(args.Source, args.NewValue));
    }

    private static void RelayInteriorChange(Element sender, ElementChangedEventArgs<byte> args)
    {
        if (!args.IsSync)
            RelayChange(sender, ElementPacketFactory.CreateSetInteriorPacket(args.Source, args.NewValue));
    }

    private static void RelayElementFrozenChanged(Element sender, ElementChangedEventArgs<bool> args)
        => RelayChange(sender, ElementPacketFactory.CreateSetElementFrozen(args.Source, args.NewValue));

    private static void RelayCollisionEnabledhanged(Element sender, ElementChangedEventArgs<bool> args)
        => RelayChange(sender, ElementPacketFactory.CreateSetCollisionsEnabledPacket(args.Source, args.NewValue));

    private static void RelayCallPropagationChanged(Element sender, ElementChangedEventArgs<bool> args)
        => RelayChange(sender, ElementPacketFactory.CreateSetCallPropagationEnabledPacket(args.Source, args.NewValue));

    private static void RelayAttached(Element sender, ElementAttachedEventArgs args)
        => RelayChange(sender, ElementPacketFactory.CreateAttachElementPacket(args.Source, args.AttachedTo, args.OffsetPosition, args.OffsetRotation));

    private static void RelayDetached(Element sender, ElementDetachedEventArgs args)
        => RelayChange(sender, ElementPacketFactory.CreateDetachElementPacket(args.Source, args.Source.Position));

    private static void RelayAttachedOffsetChanged(Element sender, ElementAttachOffsetsChangedArgs args)
        => RelayChange(sender, ElementPacketFactory.CreateSetElementAttachedOffsetsPacket(args.Source, args.OffsetPosition, args.OffsetPosition));

    private static void RelayElementDestroy(Element element)
    {
        if (element is Player)
            return;

        element.DestroyFor(element.AssociatedPlayers);
    }

    public static void RelayChange(this Element element, Packet packet)
    {
        packet.SendTo(element.AssociatedPlayers);
    }
}
