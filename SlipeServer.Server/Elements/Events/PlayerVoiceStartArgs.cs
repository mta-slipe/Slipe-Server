using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerVoiceStartArgs(Player source, byte[] dataBuffer) : EventArgs
{
    public Player Source { get; } = source;
    public byte[] DataBuffer { get; } = dataBuffer;
}
