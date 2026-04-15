using SlipeServer.Server.Elements;
using System.Collections.Generic;

namespace SlipeServer.Scripting.Definitions;

/// <summary>
/// Represents an MTA-style table/element parameter that can hold either a single element
/// (e.g. a Player or the root element) or a list of players.
/// Used by functions such as outputChatBox where the visibleTo argument accepts either form.
/// </summary>
public sealed class ElementTarget
{
    public Element? Element { get; }
    public IReadOnlyList<Player>? Players { get; }

    public ElementTarget(Element? element)
    {
        this.Element = element;
    }

    public ElementTarget(IReadOnlyList<Player> players)
    {
        this.Players = players;
    }
}
