using System;

namespace SlipeServer.Scripting
{
    public class ScriptFunctionDefinitionAttribute : Attribute
    {
        public string NiceName { get; }

        public ScriptFunctionDefinitionAttribute(string niceName)
        {
            NiceName = niceName;
        }
    }
}
