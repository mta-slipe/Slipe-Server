using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerResourceStartedEventArgs : EventArgs
{
    public Player Source { get; }
    public ushort NetId { get; }

    public PlayerResourceStartedEventArgs(
        Player source, ushort netId
    )
    {
        this.Source = source;
        this.NetId = netId;
    }
}
