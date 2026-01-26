using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerMoneyChangedEventArgs(
    Player source, int money, bool instant
    ) : EventArgs
{
    public Player Source { get; } = source;
    public int Money { get; } = money;
    public bool Instant { get; } = instant;
}
