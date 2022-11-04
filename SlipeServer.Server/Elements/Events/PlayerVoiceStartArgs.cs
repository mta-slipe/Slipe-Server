using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerVoiceStartArgs : EventArgs
{
    public Player Source { get; }
    public byte[] DataBuffer { get; }

    public PlayerVoiceStartArgs(Player source, byte[] dataBuffer)
    {
        this.Source = source;
        this.DataBuffer = dataBuffer;
    }
}
