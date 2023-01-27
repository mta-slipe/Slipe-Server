using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class ElementDataChangedArgs : EventArgs
{
    public string Key { get; }
    public LuaValue NewValue { get; }
    public LuaValue? OldValue { get; }
    public DataSyncType SyncType { get; }

    public ElementDataChangedArgs(string key, LuaValue newValue, LuaValue? oldValue = null, DataSyncType syncType = DataSyncType.Local)
    {
        this.Key = key;
        this.NewValue = newValue;
        this.OldValue = oldValue;
        this.SyncType = syncType;
    }
}
