namespace SlipeServer.Scripting;

public interface IScriptingRuntimeService
{
    string ToDebugString(object value);
    bool Compare(object a, object b);
    void Error(string message);
}
