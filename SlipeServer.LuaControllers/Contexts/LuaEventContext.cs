using SlipeServer.Server.Elements;

namespace SlipeServer.LuaControllers.Contexts;


public class LuaEventContext(Player player, Element source, string eventName)
{
    public Player Player { get; } = player;
    public Element Source { get; } = source;
    public string EventName { get; } = eventName;
}


public class LuaEventContext<TPlayer>(TPlayer player, Element source, string eventName) : LuaEventContext(player, source, eventName) where TPlayer: Player
{
    public new TPlayer Player => (base.Player as TPlayer)!;
}
