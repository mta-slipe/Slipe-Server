using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerVoiceEndArgs(Player source) : EventArgs
{
    public Player Source { get; } = source;
}
