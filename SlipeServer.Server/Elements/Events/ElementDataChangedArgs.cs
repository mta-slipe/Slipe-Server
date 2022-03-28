using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public class ElementDataChangedArgs : EventArgs
{
    public string Key { get; set; }
    public LuaValue NewValue { get; set; }
    public LuaValue? OldValue { get; set; }
    public DataSyncType SyncType { get; set; }

    public ElementDataChangedArgs(string key, LuaValue newValue, LuaValue? oldValue = null, DataSyncType syncType = DataSyncType.Local)
    {
        this.Key = key;
        this.NewValue = newValue;
        this.OldValue = oldValue;
        this.SyncType = syncType;
    }
}
