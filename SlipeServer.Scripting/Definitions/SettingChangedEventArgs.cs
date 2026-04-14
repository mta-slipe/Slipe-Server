using SlipeServer.Packets.Definitions.Lua;
using System;

namespace SlipeServer.Scripting.Definitions;

public class SettingChangedEventArgs(string setting, LuaValue newValue, LuaValue oldValue) : EventArgs
{
    public string Setting { get; } = setting;
    public LuaValue NewValue { get; } = newValue;
    public LuaValue OldValue { get; } = oldValue;
}
