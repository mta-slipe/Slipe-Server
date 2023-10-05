namespace SlipeServer.Packets.Definitions.Lua;

internal class LuaTableDebuggingView
{
    private readonly LuaTable luaTable;

    public LuaTableDebuggingView(LuaTable luaTable)
    {
        this.luaTable = luaTable;
    }

    public LuaType Type => LuaType.Table;

    public object? Value => this.luaTable.IsSequential ? $"Sequential table of Length={this.luaTable.Count}" : $"Associative table of Length={this.luaTable.Count}";
}
