using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Lua
{
    public class ScriptDefinitionAttribute : Attribute
    {
        public string NiceName { get; }

        public ScriptDefinitionAttribute(string niceName)
        {
            NiceName = niceName;
        }
    }
}
