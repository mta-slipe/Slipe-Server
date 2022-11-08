using System;

namespace SlipeServer.Server.Elements.Events;

public class PedAnimationSpeedChangedEventArgs : EventArgs
{
    public Ped Ped { get; set; }
    public string Animation { get; }
    public float Speed { get; set; }

    public PedAnimationSpeedChangedEventArgs(Ped ped, string animation, float speed)
    {
        this.Ped = ped;
        this.Animation = animation;
        this.Speed = speed;
    }
}
