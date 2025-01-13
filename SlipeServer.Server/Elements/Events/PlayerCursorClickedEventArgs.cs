using SlipeServer.Server.Elements.Enums;
using System;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerCursorClickedEventArgs(
    CursorButton button,
    bool isDown,
    Point position,
    Vector3 worldPosition,
    Element? element) : EventArgs
{
    public CursorButton Button { get; } = button;
    public bool IsDown { get; } = isDown;
    public Point Position { get; } = position;
    public Vector3 WorldPosition { get; } = worldPosition;
    public Element? Element { get; } = element;
}
