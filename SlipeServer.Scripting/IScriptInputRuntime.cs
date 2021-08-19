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
    }

    public delegate void CommandDelegate(Element element, string commandName, params string[] parameters);
}
