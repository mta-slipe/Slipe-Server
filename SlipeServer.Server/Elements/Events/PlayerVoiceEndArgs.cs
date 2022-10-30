using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events;

public class PlayerVoiceEndArgs : EventArgs
{
    public Player Source { get; }

    public PlayerVoiceEndArgs(Player source)
    {
        this.Source = source;
    }
}
