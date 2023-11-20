using SlipeServer.Packets.Definitions.Lua.ElementRpc.Marker;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Extensions.Relaying;

public static class MarkerPropertyRelayingExtensions
{
    public static void AddMarkerRelayers(this Marker marker)
    {
        marker.SizeChanged += RelaySizeChanged;
        marker.MarkerTypeChanged += RelayMarkerTypeChanged;
        marker.MarkerIconChanged += RelayMarkerIconChanged;
        marker.ColorChanged += RelayColorChanged;
        marker.TargetPositionChanged += RelayTargetPositionChanged;
    }

    private static void RelaySizeChanged(Element sender, ElementChangedEventArgs<Marker, float> args)
    {
        sender.RelayChange(new SetMarkerSizeRpcPacket(args.Source.Id, args.NewValue));
    }

    private static void RelayMarkerTypeChanged(Element sender, ElementChangedEventArgs<Marker, MarkerType> args)
    {
        sender.RelayChange(new SetMarkerTypeRpcPacket(args.Source.Id, (byte)args.NewValue));
    }

    private static void RelayMarkerIconChanged(Element sender, ElementChangedEventArgs<Marker, MarkerIcon> args)
    {
        sender.RelayChange(new SetMarkerIconRpcPacket(args.Source.Id, (byte)args.NewValue));
    }

    private static void RelayColorChanged(Element sender, ElementChangedEventArgs<Marker, Color> args)
    {
        sender.RelayChange(new SetMarkerColorRpcPacket(args.Source.Id, args.NewValue));
    }

    private static void RelayTargetPositionChanged(Element sender, ElementChangedEventArgs<Marker, Vector3?> args)
    {
        sender.RelayChange(new SetMarkerTargetPositionRpcPacket(args.Source.Id, args.NewValue));
    }
}
