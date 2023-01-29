using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerDiagnosticInfo : EventArgs
{
    public uint Level { get; }
    public string Message { get; }

    public PlayerDiagnosticInfo(uint level, string message)
    {
        this.Level = level;
        this.Message = message;
    }
}
