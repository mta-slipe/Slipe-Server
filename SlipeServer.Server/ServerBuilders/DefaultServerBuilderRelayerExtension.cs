using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using System;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.ServerBuilders;

public static class DefaultServerBuilderRelayerExtensions
{
    public static void AddDefaultRelayers(this ServerBuilder builder, ServerBuilderDefaultRelayers exceptRelayers = ServerBuilderDefaultRelayers.None)
    {
        AddDefaultElementRelayers(builder, exceptRelayers);
        AddDefaultBlipRelayers(builder, exceptRelayers);
    }

    private static void AddDefaultElementRelayers(ServerBuilder builder, ServerBuilderDefaultRelayers exceptRelayers = ServerBuilderDefaultRelayers.None)
    {
        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementPosition))
            builder.AddRelayer<Element, ElementChangedEventArgs<Vector3>>(
                (element, handler) => element.PositionChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetPositionPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementRotation))
            builder.AddRelayer<Element, ElementChangedEventArgs<Vector3>>(
                (element, handler) => element.RotationChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetRotationPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementAlpha))
            builder.AddRelayer<Element, ElementChangedEventArgs<byte>>(
                (element, handler) => element.AlphaChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetAlphaPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementDimension))
            builder.AddRelayer<Element, ElementChangedEventArgs<ushort>>(
                (element, handler) => element.DimensionChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetDimensionPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementInterior))
            builder.AddRelayer<Element, ElementChangedEventArgs<byte>>(
                (element, handler) => element.InteriorChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetInteriorPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementIsCallPropagationEnabled))
            builder.AddRelayer<Element, ElementChangedEventArgs<bool>>(
                (element, handler) => element.CallPropagationChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetCallPropagationEnabledPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementAreCollisionsEnabled))
            builder.AddRelayer<Element, ElementChangedEventArgs<bool>>(
                (element, handler) => element.CollisionEnabledhanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetCollisionsEnabledPacket(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementIsFrozenChanged))
            builder.AddRelayer<Element, ElementChangedEventArgs<bool>>(
                (element, handler) => element.FrozenChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetElementFrozen(element, args.NewValue));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementDestroyed))
            builder.AddRelayer<Element, ElementDestroyedEventArgs>(
                (element, handler) => element.Destroyed += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateRemoveEntityPacket(element));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementAttached))
            builder.AddRelayer<Element, ElementAttachedEventArgs>(
                (element, handler) => element.Attached += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateAttachElementPacket(element, args.AttachedTo, args.OffsetPosition, args.OffsetRotation));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementDetached))
            builder.AddRelayer<Element, ElementDetachedEventArgs>(
                (element, handler) => element.Detached += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateDetachElementPacket(element, args.Source.Position));

        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.ElementAttachmentOffsetChanged))
            builder.AddRelayer<Element, ElementAttachOffsetsChangedArgs>(
                (element, handler) => element.AttachedOffsetChanged += (sender, args) => handler(sender, args),
                (element, args) => ElementPacketFactory.CreateSetElementAttachedOffsetsPacket(element, args.OffsetPosition, args.OffsetRotation));
    }

    private static void AddDefaultBlipRelayers(ServerBuilder builder, ServerBuilderDefaultRelayers exceptRelayers = ServerBuilderDefaultRelayers.None)
    {
        if (!exceptRelayers.HasFlag(ServerBuilderDefaultRelayers.BlipColorChanged))
            builder.AddRelayer<Blip, ElementChangedEventArgs<Blip, Color>>(
                (blip, handler) => blip.ColorChanged += (sender, args) => handler(sender, args),
                (blip, args) => new SetBlipColorRpcPacket(blip.Id, args.NewValue));
    }
}
