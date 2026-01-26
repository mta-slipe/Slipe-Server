using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerResourceStartedEventArgs(
    Player source, ushort netId
    ) : EventArgs
{
    public Player Source { get; } = source;
    public ushort NetId { get; } = netId;
}
