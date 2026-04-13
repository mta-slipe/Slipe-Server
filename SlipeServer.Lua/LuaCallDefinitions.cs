using MoonSharp.Interpreter;
using SlipeServer.Server.Resources;
using System.Linq;

namespace SlipeServer.Lua;

public class LuaCallDefinitions(LuaEnvironmentService environmentService)
{
    [Scripting.ScriptFunctionDefinition("call")]
    public DynValue[] Call(Resource? resource, string? functionName, params DynValue[] args)
    {
        if (resource == null || functionName == null)
            return [DynValue.False];

        if (!resource.Exports.Contains(functionName))
            return [DynValue.False];

        var env = environmentService.GetEnvironment(resource);
        if (env == null)
            return [DynValue.False];

        var sourceResource = Scripting.ScriptExecutionContext.Current?.Owner;

        try
        {
            return env.CallWithSource(functionName, sourceResource, args);
        }
        catch
        {
            return [DynValue.False];
        }
    }

    [Scripting.ScriptFunctionDefinition("getResourceFromName")]
    public Resource? GetResourceFromName(string name)
    {
        return environmentService.GetAllEnvironments()
            .FirstOrDefault(e => e.ExecutionContext.Owner?.Name == name)
            ?.ExecutionContext.Owner;
    }

    [Scripting.ScriptFunctionDefinition("getThisResource")]
    public Resource? GetThisResource()
    {
        return Scripting.ScriptExecutionContext.Current?.Owner;
    }

    internal void LoadExports(Script script)
    {
        script.DoString("""
            exports = setmetatable({}, {
                __index = function(t, resourceName)
                    return setmetatable({}, {
                        __index = function(innerProxy, functionName)
                            return function(self, ...)
                                return call(getResourceFromName(resourceName), functionName, ...)
                            end
                        end
                    })
                end
            })
            """);
    }
}
