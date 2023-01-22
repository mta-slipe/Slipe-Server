using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements.Enums;

namespace SlipeServer.Server.Concepts;

/// <summary>
/// Represents an item of Lua element data
/// </summary>
public class ElementData
{
    public string Key { get; set; }
    public LuaValue Value { get; set; }
    public DataSyncType SyncType { get; set; }

    public ElementData(string key, LuaValue value, DataSyncType syncType)
    {
        this.Key = key;
        this.Value = value;
        this.SyncType = syncType;
    }
}
