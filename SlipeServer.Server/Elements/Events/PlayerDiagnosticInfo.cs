using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerDiagnosticInfo(uint level, string message) : EventArgs
{
    public uint Level { get; } = level;
    public string Message { get; } = message;
}
