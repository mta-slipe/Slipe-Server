using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedAnimationStartedEventArgs(
    Ped ped,
    string block,
    string animation,
    TimeSpan time,
    bool loops,
    bool updatesPosition,
    bool isInteruptable,
    bool freezesOnLastFrame,
    TimeSpan blendTime,
    bool retainPedState) : EventArgs
{
    public Ped Ped { get; } = ped;
    public string Block { get; } = block;
    public string Animation { get; } = animation;
    public TimeSpan Time { get; } = time;
    public bool Loops { get; } = loops;
    public bool UpdatesPosition { get; } = updatesPosition;
    public bool IsInteruptable { get; } = isInteruptable;
    public bool FreezesOnLastFrame { get; } = freezesOnLastFrame;
    public TimeSpan BlendTime { get; } = blendTime;
    public bool RetainPedState { get; } = retainPedState;
}
