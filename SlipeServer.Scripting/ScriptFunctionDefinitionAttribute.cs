using System;

namespace SlipeServer.Scripting;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ScriptFunctionDefinitionAttribute : Attribute
{
    public string NiceName { get; }

    public ScriptFunctionDefinitionAttribute(string niceName)
    {
        this.NiceName = niceName;
    }
}
