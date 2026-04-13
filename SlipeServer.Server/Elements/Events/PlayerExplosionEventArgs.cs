using SlipeServer.Server.Enums;
using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public class PlayerExplosionEventArgs(Player player, Vector3 position, ExplosionType explosionType) : EventArgs
{
    public Player Player { get; } = player;
    public Vector3 Position { get; } = position;
    public ExplosionType ExplosionType { get; } = explosionType;
}
