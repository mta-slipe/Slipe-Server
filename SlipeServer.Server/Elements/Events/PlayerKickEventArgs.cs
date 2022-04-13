using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerKickEventArgs : EventArgs
{
    public string Reason { get; set; }
    public PlayerDisconnectType Type { get; set; }

    public PlayerKickEventArgs(string reason, PlayerDisconnectType type)
    {
        this.Reason = reason;
        this.Type = type;
    }
}
