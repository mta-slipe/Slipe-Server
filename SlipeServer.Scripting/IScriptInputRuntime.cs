using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Scripting
{
    public interface IScriptInputRuntime
    {
        void AddCommandHandler(string eventName, CommandDelegate callbackDelegate);
        void RemoveCommandHandler(string eventName, CommandDelegate? callbackDelegate = null);
    }

    public delegate void CommandDelegate(Element element, string commandName, params string[] parameters);
}
