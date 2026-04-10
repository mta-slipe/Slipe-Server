using SlipeServer.Server.Enums;
using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerWeaponFiredEventArgs(Player player, WeaponId weapon, Vector3 startPosition, Vector3 endPosition, Element? hitElement) : EventArgs
{
    public Player Player { get; } = player;
    public WeaponId Weapon { get; } = weapon;
    public Vector3 StartPosition { get; } = startPosition;
    public Vector3 EndPosition { get; } = endPosition;
    public Element? HitElement { get; } = hitElement;
}
