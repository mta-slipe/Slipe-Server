using System;

namespace SlipeServer.Server.Elements.Events;

public class PedAnimationProgressChangedEventArgs : EventArgs
{
    public Ped Ped { get; set; }
    public string Animation { get; }
    public float Progress { get; set; }

    public PedAnimationProgressChangedEventArgs(Ped ped, string animation, float progress)
    {
        this.Ped = ped;
        this.Animation = animation;
        this.Progress = progress;
    }
}
