using SlipeServer.Lua;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Tools;

public static class LuaTestExtensions
{
    extension(IMtaServer server)
    {
        public void RunLuaScript(string code)
        {
            var luaService = server.GetRequiredService<LuaService>();
            luaService.LoadScript($"{Guid.NewGuid()}-test", code);
        }

        public void AddGlobal(string key, object value)
        {
            var luaService = server.GetRequiredService<LuaService>();
            luaService.AddGlobal(key, value);
        }
    }
}
