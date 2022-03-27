using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public class PedStatChangedEventArgs : EventArgs
{
    public PedStat Stat { get; set; }
    public float? OldValue { get; set; }
    public float? NewValue { get; set; }

    public PedStatChangedEventArgs(PedStat stat, float? oldValue, float? newValue)
    {
        this.Stat = stat;
        this.OldValue = oldValue;
        this.NewValue = newValue;
    }
}
