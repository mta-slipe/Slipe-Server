using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Scripting.Definitions
{
    public class InputScriptDefinitions
    {
        private readonly IScriptInputRuntime inputRuntime;

        public InputScriptDefinitions(IScriptInputRuntime inputRuntime)
        {
            this.inputRuntime = inputRuntime;
        }

        [ScriptFunctionDefinition("addCommandHandler")]
        public void AddCommandHandler(string commandName, EventDelegate callback)
        {
            this.inputRuntime.AddCommandHandler(commandName, callback);
        }
    }
}
