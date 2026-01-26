using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedStatChangedEventArgs(PedStat stat, float? oldValue, float? newValue) : EventArgs
{
    public PedStat Stat { get; } = stat;
    public float? OldValue { get; } = oldValue;
    public float? NewValue { get; } = newValue;
}
