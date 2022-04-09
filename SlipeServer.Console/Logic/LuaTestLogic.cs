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

    public LuaTestLogic(
        IScriptEventRuntime eventRuntime, 
        LuaService luaService,
        CommandService commandService)
    {
        this.eventRuntime = eventRuntime;
        this.luaService = luaService;

        commandService.AddCommand("physics").Triggered += (source, args) => Init();
    }

    private void Init()
    {
        this.eventRuntime.LoadDefaultEvents();

        this.luaService.LoadDefaultDefinitions();

        this.luaService.LoadDefinitions<CustomMathDefinition>();
        this.luaService.LoadDefinitions<TestDefinition>();

        using FileStream testLua = File.OpenRead("test.lua");
        using StreamReader reader = new StreamReader(testLua);
        try
        {
            this.luaService.LoadScript("test.lua", reader.ReadToEnd());
        }
        catch (InterpreterException ex)
        {
            System.Console.WriteLine("Failed to load script\n\t{0}", ex.DecoratedMessage);
        }
    }
}
