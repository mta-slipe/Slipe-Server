using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerQuitEventArgs(QuitReason reason) : EventArgs
{
    public QuitReason Reason { get; } = reason;
}
