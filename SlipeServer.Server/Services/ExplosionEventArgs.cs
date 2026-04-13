using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System;
using System.Numerics;

namespace SlipeServer.Server.Services;

public class ExplosionEventArgs(Vector3 position, ExplosionType type, Player? responsiblePlayer) : EventArgs
{
    public Vector3 Position { get; } = position;
    public ExplosionType Type { get; } = type;
    public Player? ResponsiblePlayer { get; } = responsiblePlayer;
}
