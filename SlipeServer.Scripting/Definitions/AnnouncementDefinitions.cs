using SlipeServer.Server;
using SlipeServer.Server.AllSeeingEye;

namespace SlipeServer.Scripting.Definitions;

public class AnnouncementDefinitions(IMtaServer server, IAseQueryService aseService)
{
    [ScriptFunctionDefinition("getGameType")]
    public string GetGameType() => server.GameType;

    [ScriptFunctionDefinition("getMapName")]
    public string GetMapName() => server.MapName;

    [ScriptFunctionDefinition("getRuleValue")]
    public string? GetRuleValue(string ruleName)
    {
        return aseService.GetRule(ruleName);
    }

    [ScriptFunctionDefinition("setRuleValue")]
    public void SetRuleValue(string ruleName, string ruleValue)
    {
        aseService.SetRule(ruleName, ruleValue);
    }
}
