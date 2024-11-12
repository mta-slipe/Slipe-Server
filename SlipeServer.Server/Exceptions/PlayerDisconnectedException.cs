using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Server.Exceptions;

public sealed class PlayerDisconnectedException : Exception
{
    public Player Player { get; }

    public PlayerDisconnectedException(Player player)
    {
        this.Player = player;
    }
}
