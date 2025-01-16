using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerKickEventArgs(
    string reason, 
    PlayerDisconnectType type, 
    Element? ResponsibleElement = null) : EventArgs
{
    public string Reason { get; } = reason;
    public PlayerDisconnectType Type { get; } = type;
    public Element? ResponsibleElement { get; } = ResponsibleElement;
}
