using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerSpawnedEventArgs(
    Player source,
    Vector3 position,
    float rotation,
    Team? team,
    ushort model,
    byte interior,
    ushort dimension) : EventArgs
{
    public Player Source { get; } = source;
    public Vector3 Position { get; } = position;
    public float Rotation { get; } = rotation;
    public Team? Team { get; } = team;
    public ushort Model { get; } = model;
    public byte Interior { get; } = interior;
    public ushort Dimension { get; } = dimension;
}
