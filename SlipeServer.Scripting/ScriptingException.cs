using System;

namespace SlipeServer.Scripting;
public class ScriptingException : Exception
{
    public ScriptingException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
