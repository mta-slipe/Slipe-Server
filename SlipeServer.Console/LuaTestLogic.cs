using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using SlipeServer.Console.LuaDefinitions;
using SlipeServer.Lua;
using SlipeServer.Scripting;
using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace SlipeServer.Console
{
    public class LuaTestLogic
    {
        public LuaTestLogic(IScriptEventRuntime eventRuntime, LuaService luaService)
        {
            eventRuntime.LoadDefaultEvents();

            luaService.LoadDefaultDefinitions();

            luaService.LoadDefinitions<CustomMathDefinition>();
            luaService.LoadDefinitions<TestDefinition>();

            using FileStream testLua = File.OpenRead("test.lua");
            using StreamReader reader = new StreamReader(testLua);
            try
            {
                luaService.LoadScript("test.lua", reader.ReadToEnd());
            }
            catch (InterpreterException ex)
            {
                System.Console.WriteLine("Failed to load script\n\t{0}", ex.DecoratedMessage);
            }
        }
    }
}
