using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements.Enums;

namespace SlipeServer.Server.Concepts;

/// <summary>
/// Represents an item of Lua element data
/// </summary>
public class ElementData(string key, LuaValue value, DataSyncType syncType)
{
    public string Key { get; set; } = key;
    public LuaValue Value { get; set; } = value;
    public DataSyncType SyncType { get; set; } = syncType;
}
