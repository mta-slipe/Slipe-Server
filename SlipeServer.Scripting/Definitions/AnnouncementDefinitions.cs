using SlipeServer.Server;

namespace SlipeServer.Scripting.Definitions;

public class AnnouncementDefinitions(IMtaServer server)
{
    [ScriptFunctionDefinition("getGameType")]
    public string GetGameType() => server.GameType;

    [ScriptFunctionDefinition("getMapName")]
    public string GetMapName() => server.MapName;
}
