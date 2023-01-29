using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedStatChangedEventArgs : EventArgs
{
    public PedStat Stat { get; }
    public float? OldValue { get; }
    public float? NewValue { get; }

    public PedStatChangedEventArgs(PedStat stat, float? oldValue, float? newValue)
    {
        this.Stat = stat;
        this.OldValue = oldValue;
        this.NewValue = newValue;
    }
}
