using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Scripting.EventDefinitions;

public interface IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime);
}
