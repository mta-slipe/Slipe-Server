using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerDiagnosticInfo : EventArgs
{
    public uint Level { get; set; }
    public string Message { get; set; }

    public PlayerDiagnosticInfo(uint level, string message)
    {
        this.Level = level;
        this.Message = message;
    }
}
