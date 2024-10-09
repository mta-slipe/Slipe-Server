using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using SlipeServer.Scripting;

namespace SlipeServer.Lua;

internal class LuaScriptingService : IScriptingService
{
    private readonly LuaService luaService;
    private readonly ILogger<LuaScriptingService> logger;

    public string Language => "Lua";

    public LuaScriptingService(LuaService luaService, ILogger<LuaScriptingService> logger)
    {
        this.luaService = luaService;
        this.logger = logger;
    }

    public IScript CreateScript()
    {
        var script = new Script(CoreModules.Preset_SoftSandbox);
        script.Options.DebugPrint = (value) =>
        {
            using var scope = this.logger.BeginScope(script);
            this.logger.LogInformation(value);
        };

        this.luaService.LoadGlobals(script);
        this.luaService.LoadDefinitions(script);

        return new LuaScript(script);
    }
}
