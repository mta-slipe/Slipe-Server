using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public class PedAnimationStartedEventArgs : EventArgs
{
    public Ped Ped { get; set; }
    public string Block { get; }
    public string Animation { get; }
    public TimeSpan Time { get; }
    public bool Loops { get; }
    public bool UpdatesPosition { get; }
    public bool IsInteruptable { get; }
    public bool FreezesOnLastFrame { get; }
    public TimeSpan BlendTime { get; }
    public bool RetainPedState { get; }

    public PedAnimationStartedEventArgs(
        Ped ped,
        string block,
        string animation,
        TimeSpan time,
        bool loops,
        bool updatesPosition,
        bool isInteruptable,
        bool freezesOnLastFrame,
        TimeSpan blendTime,
        bool retainPedState)
    {
        this.Ped = ped;
        this.Block = block;
        this.Animation = animation;
        this.Time = time;
        this.Loops = loops;
        this.UpdatesPosition = updatesPosition;
        this.IsInteruptable = isInteruptable;
        this.FreezesOnLastFrame = freezesOnLastFrame;
        this.BlendTime = blendTime;
        this.RetainPedState = retainPedState;
    }
}
