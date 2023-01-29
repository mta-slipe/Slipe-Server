using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Extensions.Relaying;

public static class RadarAreaPropertyRelayingExtensions
{
    public static void AddRadarAreaRelayers(this RadarArea radarArea)
    {
        radarArea.ColorChanged += ColorChanged;
        radarArea.SizeChanged += SizeChanged;
        radarArea.FlashingStateChanged += FlashingStateChanged;
    }

    private static void FlashingStateChanged(Element sender, ElementChangedEventArgs<RadarArea, bool> args)
    {
        sender.RelayChange(new SetRadarAreaFlashingPacket(args.Source.Id, args.NewValue));
    }

    private static void ColorChanged(Element sender, ElementChangedEventArgs<RadarArea, Color> args)
    {
        sender.RelayChange(new SetRadarAreaColorPacket(args.Source.Id, args.NewValue));
    }

    private static void SizeChanged(Element sender, ElementChangedEventArgs<RadarArea, Vector2> args)
    {
        sender.RelayChange(new SetRadarAreaSizePacket(args.Source.Id, args.NewValue));
    }
}
