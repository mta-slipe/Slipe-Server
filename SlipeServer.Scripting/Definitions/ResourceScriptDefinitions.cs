using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using System.Drawing;

namespace SlipeServer.Scripting.Definitions;

public class ResourceScriptDefinitions
{
    private readonly MtaServer server;

    public ResourceScriptDefinitions(MtaServer server)
    {
        this.server = server;
    }

    [ScriptFunctionDefinition("getResourceName")]
    public string GetResourceName(Resource resource) => resource.Name;
}
