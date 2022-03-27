using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events;

public class PlayerMoneyChangedEventArgs : EventArgs
{
    public Player Source { get; }
    public int Money { get; set; }
    public bool Instant { get; set; }

    public PlayerMoneyChangedEventArgs(
        Player source, int money, bool instant
    )
    {
        this.Source = source;
        this.Money = money;
        this.Instant = instant;
    }
}
