using SlipeServer.Server.Elements;

namespace SlipeServer.LuaControllers.Contexts;

public class LuaEventContext
{
    public Player Player { get; }
    public Element Source { get; }
    public string EventName { get; }

    public LuaEventContext(Player client, Element source, string eventName)
    {
        this.Player = client;
        this.Source = source;
        this.EventName = eventName;
    }
}
