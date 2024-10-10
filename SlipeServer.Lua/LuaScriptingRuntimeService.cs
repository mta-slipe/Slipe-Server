using MoonSharp.Interpreter;
using SlipeServer.Scripting;
using System;

namespace SlipeServer.Lua;

internal class LuaScriptingRuntimeService : IScriptingRuntimeService
{
    public string ToDebugString(object value)
    {
        return ((DynValue)value).ToDebugPrintString();
    }
    
    public bool Compare(object a, object b)
    {
        var valueA = ((DynValue)a).ToDebugPrintString();
        var valueB = ((DynValue)b).ToDebugPrintString();
        return valueA == valueB;
    }

    public void Error(string message)
    {
        var resource = ServerResourceContext.Current;

        if (resource == null)
            throw new NullReferenceException(nameof(resource));

        // TODO: Include error location

        throw new ScriptRuntimeException(message);
    }
}
