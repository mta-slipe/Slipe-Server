using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements.Enums;
using System;

namespace SlipeServer.Server.Concepts;

/// <summary>
/// Represents an item of Lua element data
/// </summary>
[Obsolete("It is highly not recommended to use element data!")]
public class ElementDataItem
{
    public string Key { get; set; }
    public LuaValue Value { get; set; }
    public DataSyncType SyncType { get; set; }

    public ElementDataItem(string key, LuaValue value, DataSyncType syncType)
    {
        this.Key = key;
        this.Value = value;
        this.SyncType = syncType;
    }
}
