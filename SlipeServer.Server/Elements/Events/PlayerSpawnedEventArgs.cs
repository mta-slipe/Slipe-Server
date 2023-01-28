using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerSpawnedEventArgs : EventArgs
{
    public Player Source { get; }

    public PlayerSpawnedEventArgs(Player source)
    {
        this.Source = source;
    }
}
