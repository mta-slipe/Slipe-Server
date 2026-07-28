using MoonSharp.Interpreter;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using System;
using System.Linq;

namespace SlipeServer.Lua;

public class LuaCallDefinitions(LuaEnvironmentService environmentService, IResourceProvider? resourceProvider = null)
{
    [Scripting.ScriptFunctionDefinition("call")]
    public DynValue[] Call(IServerSideResource? resource, string? functionName, params DynValue[] args)
    {
        if (resource == null || functionName == null)
            return [DynValue.False];

        if (!resource.ServerExports.Contains(functionName))
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
            .FirstOrDefault(e => string.Equals(e.ExecutionContext.Owner?.Name, name, StringComparison.OrdinalIgnoreCase))
            ?.ExecutionContext.Owner
            ?? resourceProvider?.GetResources().FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    [Scripting.ScriptFunctionDefinition("getThisResource")]
    public Resource? GetThisResource()
    {
        return Scripting.ScriptExecutionContext.Current?.Owner;
    }

    [Scripting.ScriptFunctionDefinition("getResourceName")]
    public string? GetResourceName(Resource resource)
    {
        return resource.Name;
    }

    [Scripting.ScriptFunctionDefinition("getResourceRootElement")]
    public Server.Elements.Element? GetResourceRootElement(Resource? resource = null)
    {
        resource ??= Scripting.ScriptExecutionContext.Current?.Owner;
        return resource?.Root;
    }

    [Scripting.ScriptFunctionDefinition("getResourceInfo")]
    public string? GetResourceInfo(Resource resource, string attribute)
    {
        resource.Info.TryGetValue(attribute, out var value);
        return value;
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
