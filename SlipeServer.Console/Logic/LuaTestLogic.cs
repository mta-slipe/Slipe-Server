using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using SlipeServer.Console.LuaDefinitions;
using SlipeServer.Lua;
using SlipeServer.Scripting;
using SlipeServer.Server.Services;
using System.IO;

namespace SlipeServer.Console.Logic;

public class LuaTestLogic
{
    private readonly IScriptEventRuntime eventRuntime;
    private readonly LuaService luaService;
    private readonly ILogger logger;

    public LuaTestLogic(
        IScriptEventRuntime eventRuntime, 
        LuaService luaService,
        CommandService commandService,
        ILogger logger
        )
    {
        this.eventRuntime = eventRuntime;
        this.luaService = luaService;
        this.logger = logger;
        //commandService.AddCommand("lua").Triggered += (source, args) => Init();
        Init();
    }

    private void Init()
    {
        this.eventRuntime.LoadDefaultEvents();

        using FileStream testLua = File.OpenRead("test.lua");
        using StreamReader reader = new StreamReader(testLua);
        try
        {
            Script createdScript = this.luaService.LoadScript("test.lua", reader.ReadToEnd());

            this.luaService.LoadDefaultDefinitions(createdScript);
            this.luaService.LoadDefinitions<CustomMathDefinition>(createdScript);
            this.luaService.LoadDefinitions<TestDefinition>(createdScript);

        }
        catch (InterpreterException ex)
        {
            this.logger.LogInformation("Failed to load script\n\t{message}", ex.DecoratedMessage);
        }
    }
}
