namespace SlipeServer.Packets.Definitions.Lua;

internal class LuaValueDebuggingView(LuaValue luaValue)
{
    public LuaType Type => luaValue.LuaType;

    public object? Value => luaValue.LuaType switch
    {
        LuaType.None => "none",
        LuaType.Nil => "nil",
        LuaType.Boolean => luaValue.BoolValue,
        LuaType.Number => luaValue.IntegerValue ?? luaValue.FloatValue ?? luaValue.DoubleValue,
        LuaType.Userdata => luaValue.ElementId,
        LuaType.String or LuaType.LongString => luaValue.StringValue,
        LuaType.Table => luaValue.TableValue,
        _ => $"Unsupported Lua value type: {luaValue.LuaType}.",
    };
}
