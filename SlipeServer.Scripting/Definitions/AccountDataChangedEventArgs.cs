using SlipeServer.Packets.Definitions.Lua;
using System;

namespace SlipeServer.Scripting.Definitions;

public class AccountDataChangedEventArgs(AccountHandle account, string key, LuaValue newValue, LuaValue oldValue) : EventArgs
{
    public AccountHandle Account { get; } = account;
    public string Key { get; } = key;
    public LuaValue NewValue { get; } = newValue;
    public LuaValue OldValue { get; } = oldValue;
}
