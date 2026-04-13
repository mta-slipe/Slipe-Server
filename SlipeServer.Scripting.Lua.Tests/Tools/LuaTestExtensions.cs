using SlipeServer.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Resources;

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

        public LuaEnvironment CreateEnvironment(string identifier, Resource? resource = null)
        {
            var luaService = server.GetRequiredService<LuaService>();
            return luaService.CreateEnvironment(identifier, resource);
        }

        public LuaEnvironmentService GetEnvironmentService()
        {
            return server.GetRequiredService<LuaEnvironmentService>();
        }
    }
}
