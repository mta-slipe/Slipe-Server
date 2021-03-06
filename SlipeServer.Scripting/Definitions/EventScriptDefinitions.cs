using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Scripting.Definitions
{
    public class EventScriptDefinitions
    {
        private readonly IScriptEventRuntime eventRuntime;

        public EventScriptDefinitions(IScriptEventRuntime eventRuntime)
        {
            this.eventRuntime = eventRuntime;
        }

        [ScriptFunctionDefinition("addEventHandler")]
        public void AddEventHandler(string eventName, Element attachedTo, EventDelegate callback)
        {
            this.eventRuntime.AddEventHandler(eventName, attachedTo, callback);
        }

        [ScriptFunctionDefinition("removeEventHandler")]
        public void RemoveEventHandler(string eventName, Element attachedTo, EventDelegate callback)
        {
            this.eventRuntime.RemoveEventHandler(eventName, attachedTo, callback);
        }
    }
}
