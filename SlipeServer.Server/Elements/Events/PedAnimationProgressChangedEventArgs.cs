using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedAnimationProgressChangedEventArgs : EventArgs
{
    public Ped Ped { get; }
    public string Animation { get; }
    public float Progress { get; }

    public PedAnimationProgressChangedEventArgs(Ped ped, string animation, float progress)
    {
        this.Ped = ped;
        this.Animation = animation;
        this.Progress = progress;
    }
}
