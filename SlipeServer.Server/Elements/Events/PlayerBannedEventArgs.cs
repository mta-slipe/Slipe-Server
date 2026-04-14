using SlipeServer.Server.Bans;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerBannedEventArgs(Ban ban, Element? responsibleElement = null) : EventArgs
{
    public Ban Ban { get; } = ban;
    public Element? ResponsibleElement { get; } = responsibleElement;
}
