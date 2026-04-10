using SlipeServer.Server.Elements.Enums;
using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class ElementClickedEventArgs(Player player, CursorButton button, bool isDown, Vector3 worldPosition) : EventArgs
{
    public Player Player { get; } = player;
    public CursorButton Button { get; } = button;
    public bool IsDown { get; } = isDown;
    public Vector3 WorldPosition { get; } = worldPosition;
}
