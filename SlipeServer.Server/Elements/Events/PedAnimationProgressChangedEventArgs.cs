using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedAnimationProgressChangedEventArgs(Ped ped, string animation, float progress) : EventArgs
{
    public Ped Ped { get; } = ped;
    public string Animation { get; } = animation;
    public float Progress { get; } = progress;
}
