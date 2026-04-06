using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using System.Linq;

namespace SlipeServer.Scripting.Definitions;

public class ElementDataScriptDefinitions
{
    [ScriptFunctionDefinition("getElementData")]
    public LuaValue? GetElementData(Element element, string key, bool inherit = true)
        => element.GetData(key, inherit);

    [ScriptFunctionDefinition("setElementData")]
    public bool SetElementData(Element element, string key, LuaValue value, string syncMode = "broadcast")
    {
        var syncType = syncMode switch
        {
            "local" => DataSyncType.Local,
            "subscribe" => DataSyncType.Subscribe,
            _ => DataSyncType.Broadcast
        };

        element.SetData(key, value, syncType);
        return true;
    }

    [ScriptFunctionDefinition("removeElementData")]
    public bool RemoveElementData(Element element, string key)
    {
        element.SetData(key, LuaValue.Nil);
        return true;
    }

    [ScriptFunctionDefinition("hasElementData")]
    public bool HasElementData(Element element, string key, bool inherit = true)
        => element.GetData(key, inherit) is not null;

    [ScriptFunctionDefinition("getAllElementData")]
    public LuaValue GetAllElementData(Element element)
    {
        var data = element.GetAllData();
        return new LuaValue(data.ToDictionary(
            kvp => new LuaValue(kvp.Key),
            kvp => kvp.Value));
    }

    [ScriptFunctionDefinition("addElementDataSubscriber")]
    public bool AddElementDataSubscriber(Element element, string key, Player player)
    {
        element.SubscribeToData(player, key);
        return true;
    }

    [ScriptFunctionDefinition("removeElementDataSubscriber")]
    public bool RemoveElementDataSubscriber(Element element, string key, Player player)
    {
        element.UnsubscribeFromData(player, key);
        return true;
    }

    [ScriptFunctionDefinition("hasElementDataSubscriber")]
    public bool HasElementDataSubscriber(Element element, string key, Player player)
        => element.IsPlayerSubscribedToData(player, key);
}
