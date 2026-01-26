using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class ElementDataChangedArgs(string key, LuaValue newValue, LuaValue? oldValue = null, DataSyncType syncType = DataSyncType.Local) : EventArgs
{
    public string Key { get; } = key;
    public LuaValue NewValue { get; } = newValue;
    public LuaValue? OldValue { get; } = oldValue;
    public DataSyncType SyncType { get; } = syncType;
}
