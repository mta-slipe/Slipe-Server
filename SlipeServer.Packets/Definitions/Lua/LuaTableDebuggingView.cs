namespace SlipeServer.Packets.Definitions.Lua;

internal class LuaTableDebuggingView(LuaTable luaTable)
{
    public LuaType Type => LuaType.Table;

    public object? Value => luaTable.IsSequential ? $"Sequential table of Length={luaTable.Count}" : $"Associative table of Length={luaTable.Count}";
}
