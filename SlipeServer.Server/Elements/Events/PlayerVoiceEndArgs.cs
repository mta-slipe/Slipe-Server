using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerVoiceEndArgs : EventArgs
{
    public Player Source { get; }

    public PlayerVoiceEndArgs(Player source)
    {
        this.Source = source;
    }
}
