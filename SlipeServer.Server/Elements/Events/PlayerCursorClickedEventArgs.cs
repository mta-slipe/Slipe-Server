using System;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public class PlayerCursorClickedEventArgs : EventArgs
{
    public byte Button { get; }
    public Point Position { get; }
    public Vector3 WorldPosition { get; }
    public Element? Element { get; }

    public PlayerCursorClickedEventArgs(
        byte button,
        Point position,
        Vector3 worldPosition,
        Element? element)
    {
        this.Button = button;
        this.Position = position;
        this.WorldPosition = worldPosition;
        this.Element = element;
    }
}
