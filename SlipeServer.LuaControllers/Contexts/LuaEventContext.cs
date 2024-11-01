using SlipeServer.Server.Elements;

namespace SlipeServer.LuaControllers.Contexts;

public class LuaEventContext
{
    public Player Player { get; }
    public Element Source { get; }
    public string EventName { get; }

    public LuaEventContext(Player player, Element source, string eventName)
    {
        this.Player = player;
        this.Source = source;
        this.EventName = eventName;
    }
}

public class LuaEventContext<TPlayer> : LuaEventContext where TPlayer: Player
{
    public new TPlayer Player => (base.Player as TPlayer)!;

    public LuaEventContext(TPlayer player, Element source, string eventName) : base(player, source, eventName)
    {
    }
}
