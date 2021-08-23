using Microsoft.Extensions.Logging;
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

            using(FileStream testLua = File.OpenRead("test.lua"))
            {
                using(StreamReader reader = new StreamReader(testLua))
                {
                    luaService.LoadScript("test.lua", reader.ReadToEnd(), new List<Type> { typeof(CustomMathDefinition) });
                }
            }
        }
    }
}
