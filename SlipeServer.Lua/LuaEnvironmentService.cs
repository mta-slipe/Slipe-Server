using SlipeServer.Scripting;
using SlipeServer.Server.Resources;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Lua;

public class LuaEnvironmentService
{
    private readonly ConcurrentDictionary<string, LuaEnvironment> environments = [];

    public void RegisterEnvironment(LuaEnvironment environment)
    {
        this.environments[environment.Identifier] = environment;
    }

    public void UnregisterEnvironment(string identifier)
    {
        this.environments.Remove(identifier, out var _);
    }

    public LuaEnvironment? GetEnvironment(string identifier)
    {
        return this.environments.TryGetValue(identifier, out var env) ? env : null;
    }

    public LuaEnvironment? GetEnvironment(Resource resource)
    {
        return this.environments.Values.FirstOrDefault(e => e.ExecutionContext.Owner == resource);
    }

    public LuaEnvironment? GetEnvironment(ScriptExecutionContext context)
    {
        return this.environments.Values.FirstOrDefault(e => e.ExecutionContext == context);
    }

    public IEnumerable<LuaEnvironment> GetAllEnvironments()
    {
        return this.environments.Values;
    }
}
