using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerSpawnedEventArgs : EventArgs
{
    public Player Source { get; }

    public PlayerSpawnedEventArgs(Player source)
    {
        this.Source = source;
    }
}
