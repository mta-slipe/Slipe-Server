using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedAnimationSpeedChangedEventArgs : EventArgs
{
    public Ped Ped { get; }
    public string Animation { get; }
    public float Speed { get; }

    public PedAnimationSpeedChangedEventArgs(Ped ped, string animation, float speed)
    {
        this.Ped = ped;
        this.Animation = animation;
        this.Speed = speed;
    }
}
