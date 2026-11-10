using SlipeServer.Server;
using SlipeServer.Server.AllSeeingEye;

namespace SlipeServer.Scripting.Definitions;

public class AnnouncementDefinitions(IMtaServer server, IAseQueryService aseService)
{
    [ScriptFunctionDefinition("getGameType")]
    public string GetGameType() => server.GameType;

    [ScriptFunctionDefinition("setGameType")]
    public bool SetGameType(string gameType)
    {
        server.GameType = gameType;
        return true;
    }

    [ScriptFunctionDefinition("getMapName")]
    public string GetMapName() => server.MapName;

    [ScriptFunctionDefinition("setMapName")]
    public bool SetMapName(string mapName)
    {
        server.MapName = mapName;
        return true;
    }

    [ScriptFunctionDefinition("getRuleValue")]
    public string? GetRuleValue(string ruleName)
    {
        return aseService.GetRule(ruleName);
    }

    [ScriptFunctionDefinition("setRuleValue")]
    public bool SetRuleValue(string ruleName, string ruleValue)
    {
        aseService.SetRule(ruleName, ruleValue);
        return true;
    }

    [ScriptFunctionDefinition("removeRuleValue")]
    public bool RemoveRuleValue(string ruleName)
    {
        return aseService.RemoveRule(ruleName);
    }
}
