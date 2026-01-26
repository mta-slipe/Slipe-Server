using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedAnimationSpeedChangedEventArgs(Ped ped, string animation, float speed) : EventArgs
{
    public Ped Ped { get; } = ped;
    public string Animation { get; } = animation;
    public float Speed { get; } = speed;
}
