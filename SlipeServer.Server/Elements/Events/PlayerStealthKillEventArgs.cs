using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerStealthKillEventArgs(Ped target) : EventArgs
{
    public Ped Target { get; } = target;
}
