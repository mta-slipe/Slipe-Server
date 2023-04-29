namespace SlipeServer.Packets.Definitions.Lua;

internal class LuaValueDebuggingView
{
    private readonly LuaValue luaValue;

    public LuaValueDebuggingView(LuaValue luaValue)
    {
        this.luaValue = luaValue;
    }
    public LuaType Type => this.luaValue.LuaType;

    public object? Value => this.luaValue.LuaType switch
    {
        LuaType.None => "none",
        LuaType.Nil => "nil",
        LuaType.Boolean => this.luaValue.BoolValue,
        LuaType.Number => this.luaValue.IntegerValue ?? this.luaValue.FloatValue ?? this.luaValue.DoubleValue,
        LuaType.Userdata => this.luaValue.ElementId,
        LuaType.String or LuaType.LongString => this.luaValue.StringValue,
        LuaType.Table => this.luaValue.TableValue,
        _ => $"Unsupported Lua value type: {this.luaValue.LuaType}.",
    };
}
