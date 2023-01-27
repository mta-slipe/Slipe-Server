using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerKickEventArgs : EventArgs
{
    public string Reason { get; }
    public PlayerDisconnectType Type { get; }

    public PlayerKickEventArgs(string reason, PlayerDisconnectType type)
    {
        this.Reason = reason;
        this.Type = type;
    }
}
