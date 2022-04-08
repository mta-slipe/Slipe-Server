using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events;

public class PlayerQuitEventArgs : EventArgs
{
    public QuitReason Reason { get; }

    public PlayerQuitEventArgs(QuitReason reason)
    {
        this.Reason = reason;
    }
}
