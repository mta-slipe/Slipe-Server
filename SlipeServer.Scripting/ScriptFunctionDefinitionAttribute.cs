using System;

namespace SlipeServer.Scripting;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ScriptFunctionDefinitionAttribute(string niceName) : Attribute
{
    public string NiceName { get; } = niceName;
}
