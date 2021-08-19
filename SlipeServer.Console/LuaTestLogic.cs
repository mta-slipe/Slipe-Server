using SlipeServer.Lua;
using SlipeServer.Scripting;
using System;
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
                    luaService.LoadScript("test.lua", reader.ReadToEnd());
                }
            }
        }
    }
}
