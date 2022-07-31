using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Server.Relayers;
public class ConfigurablePropertyRelayer<TElement, TEventArgs>
    where TElement : Element
{
    private readonly Action<TElement, Action<TElement, TEventArgs>> registerHandlerCallback;
    private readonly Func<TElement, TEventArgs, Packet?> packetCallback;
    private readonly MtaServer server;
    private readonly bool onlyWhenNotSync;

    public ConfigurablePropertyRelayer(
        Action<TElement, Action<TElement, TEventArgs>> registerHandlerCallback,
        Func<TElement, TEventArgs, Packet?> packetCallback,
        MtaServer server,
        bool onlyWhenNotSync = true
    )
    {
        this.registerHandlerCallback = registerHandlerCallback;
        this.packetCallback = packetCallback;
        this.server = server;
        this.onlyWhenNotSync = onlyWhenNotSync;
        server.ForAny<TElement>(HandleElement);
    }

    private void HandleElement(TElement element)
    {
        this.registerHandlerCallback.Invoke(element, HandleChange);
    }

    private void HandleChange(TElement source, TEventArgs eventArgs)
    {
        if (source.IsSync && this.onlyWhenNotSync)
            return;

        var packet = this.packetCallback.Invoke(source, eventArgs);
        if (packet != null)
            this.server.BroadcastPacket(packet);
    }
}
