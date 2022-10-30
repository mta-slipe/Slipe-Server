using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System.Drawing;

namespace SlipeServer.Server.Behaviour;

public class BlipBehaviour
{
    private readonly MtaServer server;

    public BlipBehaviour(MtaServer server)
    {
        this.server = server;
        this.server.ForAny<Blip>(AddBlipHandlers);
    }

    private void AddBlipHandlers(Blip blip)
    {
        blip.OrderingChanged += HandleOrderingChanged;
        blip.VisibleDistanceChanged += HandleVisibleDistanceChanged;
        blip.IconChanged += HandleIconChanged;
        blip.SizeChanged += HandleSizeChanged;
        blip.ColorChanged += HandleColorChanged;
    }

    private void HandleColorChanged(Element sender, ElementChangedEventArgs<Blip, Color> args)
    {
        this.server.BroadcastPacket(new SetBlipColorRpcPacket(args.Source.Id, args.NewValue));
    }

    private void HandleSizeChanged(Element sender, ElementChangedEventArgs<Blip, byte> args)
    {
        this.server.BroadcastPacket(new SetBlipSizeRpcPacket(args.Source.Id, args.NewValue));
    }

    private void HandleIconChanged(Element sender, ElementChangedEventArgs<Blip, BlipIcon> args)
    {
        this.server.BroadcastPacket(new SetBlipIconRpcPacket(args.Source.Id, (byte)args.NewValue));
    }

    private void HandleVisibleDistanceChanged(Element sender, ElementChangedEventArgs<Blip, ushort> args)
    {
        this.server.BroadcastPacket(new SetBlipVisibleDistanceRpcPacket(args.Source.Id, args.NewValue));
    }

    private void HandleOrderingChanged(Element sender, ElementChangedEventArgs<Blip, short> args)
    {
        this.server.BroadcastPacket(new SetBlipOrderingRpcPacket(args.Source.Id, args.NewValue));
    }
}
