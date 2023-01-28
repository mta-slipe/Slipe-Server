using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerMoneyChangedEventArgs : EventArgs
{
    public Player Source { get; }
    public int Money { get; }
    public bool Instant { get; }

    public PlayerMoneyChangedEventArgs(
        Player source, int money, bool instant
    )
    {
        this.Source = source;
        this.Money = money;
        this.Instant = instant;
    }
}
