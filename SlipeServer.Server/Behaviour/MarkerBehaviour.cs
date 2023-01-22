using SlipeServer.Packets.Definitions.Lua.ElementRpc.Marker;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.ElementCollections;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for relaying marker property changes
/// </summary>
public class MarkerBehaviour
{
    private readonly MtaServer server;

    public MarkerBehaviour(MtaServer server, IElementCollection elementCollection)
    {
        this.server = server;
        foreach (var marker in elementCollection.GetByType<Marker>(ElementType.Marker))
        {
            AddMarker(marker);
        }

        server.ElementCreated += OnElementCreate;
    }

    private void OnElementCreate(Element element)
    {
        if (element is Marker marker)
        {
            AddMarker(marker);
        }
    }

    private void AddMarker(Marker marker)
    {
        marker.SizeChanged += RelaySizeChanged;
        marker.MarkerTypeChanged += RelayMarkerTypeChanged;
        marker.MarkerIconChanged += RelayMarkerIconChanged;
        marker.ColorChanged += RelayColorChanged;
        marker.TargetPositionChanged += RelayTargetPositionChanged;
    }

    private void RelaySizeChanged(Element sender, ElementChangedEventArgs<Marker, float> args)
    {
        this.server.BroadcastPacket(new SetMarkerSizeRpcPacket(args.Source.Id, args.NewValue));
    }

    private void RelayMarkerTypeChanged(Element sender, ElementChangedEventArgs<Marker, MarkerType> args)
    {
        this.server.BroadcastPacket(new SetMarkerTypeRpcPacket(args.Source.Id, (byte)args.NewValue));
    }

    private void RelayMarkerIconChanged(Element sender, ElementChangedEventArgs<Marker, MarkerIcon> args)
    {
        this.server.BroadcastPacket(new SetMarkerIconRpcPacket(args.Source.Id, (byte)args.NewValue));
    }

    private void RelayColorChanged(Element sender, ElementChangedEventArgs<Marker, Color> args)
    {
        this.server.BroadcastPacket(new SetMarkerColorRpcPacket(args.Source.Id, args.NewValue));
    }

    private void RelayTargetPositionChanged(Element sender, ElementChangedEventArgs<Marker, Vector3?> args)
    {
        this.server.BroadcastPacket(new SetMarkerTargetPositionRpcPacket(args.Source.Id, args.NewValue));
    }
}
