using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedDamagedEventArgs(Ped source, float loss) : EventArgs
{
    public Ped Source { get; } = source;
    public float Loss { get; } = loss;
}
