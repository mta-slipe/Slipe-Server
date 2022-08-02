using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Marker;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.ServerBuilders;

public static class DefaultServerBuilderRelayerExtensions
{
    public static void AddDefaultRelayers(
        this ServerBuilder builder, ServerBuilderDefaultRelayers? exceptRelayers = null)
    {
        ServerBuilderDefaultRelayers exceptions = exceptRelayers ?? ServerBuilderDefaultRelayers.None;
        AddDefaultElementRelayers(builder, exceptions.Element);
        AddDefaultBlipRelayers(builder, exceptions.Blip);
        AddDefaultMarkerRelayers(builder, exceptions.Marker);
    }

    private static void AddDefaultElementRelayers(ServerBuilder builder, ServerBuilderDefaultElementRelayers exceptRelayers = ServerBuilderDefaultElementRelayers.None)
    {
        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementPosition))
            builder.AddRelayer<Element, ElementChangedEventArgs<Vector3>>(
                (element, handler) => element.PositionChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetPositionPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementRotation))
            builder.AddRelayer<Element, ElementChangedEventArgs<Vector3>>(
                (element, handler) => element.RotationChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetRotationPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementAlpha))
            builder.AddRelayer<Element, ElementChangedEventArgs<byte>>(
                (element, handler) => element.AlphaChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetAlphaPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementDimension))
            builder.AddRelayer<Element, ElementChangedEventArgs<ushort>>(
                (element, handler) => element.DimensionChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetDimensionPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementInterior))
            builder.AddRelayer<Element, ElementChangedEventArgs<byte>>(
                (element, handler) => element.InteriorChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetInteriorPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementIsCallPropagationEnabled))
            builder.AddRelayer<Element, ElementChangedEventArgs<bool>>(
                (element, handler) => element.CallPropagationChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetCallPropagationEnabledPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementAreCollisionsEnabled))
            builder.AddRelayer<Element, ElementChangedEventArgs<bool>>(
                (element, handler) => element.CollisionEnabledhanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetCollisionsEnabledPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementIsFrozenChanged))
            builder.AddRelayer<Element, ElementChangedEventArgs<bool>>(
                (element, handler) => element.FrozenChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetElementFrozen(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementDestroyed))
            builder.AddRelayer<Element, ElementDestroyedEventArgs>(
                (element, handler) => element.Destroyed += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateRemoveEntityPacket(element));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementAttached))
            builder.AddRelayer<Element, ElementAttachedEventArgs>(
                (element, handler) => element.Attached += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateAttachElementPacket(element, args.AttachedTo, args.OffsetPosition, args.OffsetRotation));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementDetached))
            builder.AddRelayer<Element, ElementDetachedEventArgs>(
                (element, handler) => element.Detached += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateDetachElementPacket(element, args.Source.Position));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultElementRelayers.ElementAttachmentOffsetChanged))
            builder.AddRelayer<Element, ElementAttachOffsetsChangedArgs>(
                (element, handler) => element.AttachedOffsetChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetElementAttachedOffsetsPacket(element, args.OffsetPosition, args.OffsetRotation));
    }

    private static void AddDefaultBlipRelayers(ServerBuilder builder, ServerBuilderDefaultBlipRelayers exceptRelayers = ServerBuilderDefaultBlipRelayers.None)
    {
        if (!exceptRelayers.HasFlag(ServerBuilderDefaultBlipRelayers.BlipColorChanged))
            builder.AddRelayer<Blip, ElementChangedEventArgs<Blip, Color>>(
                (blip, handler) => blip.ColorChanged += (sender, args) => handler(sender, args),
                (blip, args) => new SetBlipColorRpcPacket(blip.Id, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultBlipRelayers.BlipSizeChanged))
            builder.AddRelayer<Blip, ElementChangedEventArgs<Blip, byte>>(
                (blip, handler) => blip.SizeChanged += (sender, args) => handler(sender, args),
                (blip, args) => new SetBlipSizeRpcPacket(blip.Id, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultBlipRelayers.BlipIconChanged))
            builder.AddRelayer<Blip, ElementChangedEventArgs<Blip, BlipIcon>>(
                (blip, handler) => blip.IconChanged += (sender, args) => handler(sender, args),
                (blip, args) => new SetBlipIconRpcPacket(blip.Id, (byte)args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultBlipRelayers.BlipVisibleDistanceChanged))
            builder.AddRelayer<Blip, ElementChangedEventArgs<Blip, ushort>>(
                (blip, handler) => blip.VisibleDistanceChanged += (sender, args) => handler(sender, args),
                (blip, args) => new SetBlipVisibleDistanceRpcPacket(blip.Id, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultBlipRelayers.BlipOrderingChanged))
            builder.AddRelayer<Blip, ElementChangedEventArgs<Blip, short>>(
                (blip, handler) => blip.OrderingChanged += (sender, args) => handler(sender, args),
                (blip, args) => new SetBlipOrderingRpcPacket(blip.Id, args.NewValue));
    }

    private static void AddDefaultMarkerRelayers(ServerBuilder builder, ServerBuilderDefaultMarkerRelayers exceptRelayers = ServerBuilderDefaultMarkerRelayers.None)
    {
        if (!exceptRelayers.HasFlag(ServerBuilderDefaultMarkerRelayers.MarkerTypeChanged))
            builder.AddRelayer<Marker, ElementChangedEventArgs<Marker, MarkerType>>(
                (marker, handler) => marker.MarkerTypeChanged += (sender, args) => handler(sender, args),
                (marker, args) => new SetMarkerTypeRpcPacket(marker.Id, (byte)args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultMarkerRelayers.MarkerIconChanged))
            builder.AddRelayer<Marker, ElementChangedEventArgs<Marker, MarkerIcon>>(
                (marker, handler) => marker.MarkerIconChanged += (sender, args) => handler(sender, args),
                (marker, args) => new SetMarkerIconRpcPacket(marker.Id, (byte)args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultMarkerRelayers.MarkerSizeChanged))
            builder.AddRelayer<Marker, ElementChangedEventArgs<Marker, float>>(
                (marker, handler) => marker.SizeChanged += (sender, args) => handler(sender, args),
                (marker, args) => new SetMarkerSizeRpcPacket(marker.Id, (byte)args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultMarkerRelayers.MarkerColorChanged))
            builder.AddRelayer<Marker, ElementChangedEventArgs<Marker, Color>>(
                (marker, handler) => marker.ColorChanged += (sender, args) => handler(sender, args),
                (marker, args) => new SetMarkerColorRpcPacket(marker.Id, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultMarkerRelayers.MarkerTargetPositionChanged))
            builder.AddRelayer<Marker, ElementChangedEventArgs<Marker, Vector3?>>(
                (marker, handler) => marker.TargetPositionChanged += (sender, args) => handler(sender, args),
                (marker, args) => new SetMarkerTargetPositionRpcPacket(marker.Id, args.NewValue));
    }
}
