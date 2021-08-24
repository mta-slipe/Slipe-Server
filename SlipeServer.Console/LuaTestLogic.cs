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

            luaService.LoadDefinitions<CustomMathDefinition>();

            using (FileStream testLua = File.OpenRead("test.lua"))
            {
                using(StreamReader reader = new StreamReader(testLua))
                {
                    luaService.LoadScript("test.lua", reader.ReadToEnd(), (error) =>
                    {
                        System.Console.WriteLine("Failed to load script\n\t{0}", error);
                    });
                }
            }
        }
    }
}
