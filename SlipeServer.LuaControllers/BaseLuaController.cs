using SlipeServer.LuaControllers.Contexts;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;

namespace SlipeServer.LuaControllers;

public class BaseLuaController
{
    private readonly AsyncLocal<LuaEventContext?> context = new();

    public LuaEventContext Context
    {
        get
        {
            if (this.context.Value == null)
                throw new Exception("Can not access BaseLuaController.Context outside of event handling methods.");

            return this.context.Value;
        }
    }

    internal void SetContext(LuaEventContext? context)
    {
        this.context.Value = context;
    }

    internal virtual object? HandleEvent(LuaEvent luaEvent, Func<LuaValue[], object?> handler)
    {
        this.SetContext(new LuaEventContext(luaEvent.Player, luaEvent.Source, luaEvent.Name));
        var result = handler.Invoke(luaEvent.Parameters);
        this.SetContext(null);
        return result;
    }
}


public class BaseLuaController<TPlayer> : BaseLuaController where TPlayer : Player
{
    public new LuaEventContext<TPlayer> Context => (base.Context as LuaEventContext<TPlayer>)!;

    internal override object? HandleEvent(LuaEvent luaEvent, Func<LuaValue[], object?> handler)
    {
        if (luaEvent.Player is not TPlayer tPlayer)
            return null;

        this.SetContext(new LuaEventContext<TPlayer>(tPlayer, luaEvent.Source, luaEvent.Name));
        var result = handler.Invoke(luaEvent.Parameters);
        this.SetContext(null);
        return result;
    }
}
