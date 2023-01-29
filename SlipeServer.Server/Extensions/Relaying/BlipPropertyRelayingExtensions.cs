using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System.Drawing;

namespace SlipeServer.Server.Extensions.Relaying;

public static class BlipPropertyRelayingExtensions
{
    public static void AddBlipRelayers(this Blip blip)
    {
        blip.OrderingChanged += HandleOrderingChanged;
        blip.VisibleDistanceChanged += HandleVisibleDistanceChanged;
        blip.IconChanged += HandleIconChanged;
        blip.SizeChanged += HandleSizeChanged;
        blip.ColorChanged += HandleColorChanged;
    }

    private static void HandleOrderingChanged(Element sender, ElementChangedEventArgs<Blip, short> args)
    {
        sender.RelayChange(new SetBlipOrderingRpcPacket(args.Source.Id, args.NewValue));
    }

    private static void HandleVisibleDistanceChanged(Element sender, ElementChangedEventArgs<Blip, ushort> args)
    {
        sender.RelayChange(new SetBlipVisibleDistanceRpcPacket(args.Source.Id, args.NewValue));
    }

    private static void HandleIconChanged(Element sender, ElementChangedEventArgs<Blip, BlipIcon> args)
    {
        sender.RelayChange(new SetBlipIconRpcPacket(args.Source.Id, (byte)args.NewValue));
    }

    private static void HandleSizeChanged(Element sender, ElementChangedEventArgs<Blip, byte> args)
    {
        sender.RelayChange(new SetBlipSizeRpcPacket(args.Source.Id, args.NewValue));
    }

    private static void HandleColorChanged(Element sender, ElementChangedEventArgs<Blip, Color> args)
    {
        sender.RelayChange(new SetBlipColorRpcPacket(args.Source.Id, args.NewValue));
    }
}
