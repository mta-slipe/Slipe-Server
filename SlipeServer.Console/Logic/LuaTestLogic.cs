﻿using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using SlipeServer.Console.LuaDefinitions;
using SlipeServer.Lua;
using SlipeServer.Scripting;
using SlipeServer.Server.Services;
using System.IO;
using System.Threading.Tasks;

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
        commandService.AddCommand("lua").Triggered += (source, args) => Init();
        Task.Run(async () =>
        {
            await Task.Delay(1000);
            Init();
        });
    }

    private void Init()
    {
        this.eventRuntime.LoadDefaultEvents();

        this.luaService.LoadDefaultDefinitions();

        this.luaService.LoadDefinitions<CustomMathDefinition>();
        this.luaService.LoadDefinitions<TestDefinition>();

        using var testLua = File.OpenRead("test.lua");
        using var reader = new StreamReader(testLua);
        try
        {
            this.luaService.LoadScript("test.lua", reader.ReadToEnd());
        }
        catch (InterpreterException ex)
        {
            this.logger.LogInformation("Failed to load script\n\t{message}", ex.DecoratedMessage);
        }
    }
}
