namespace SlipeServer.Scripting;

public interface IScriptingService
{
    string Language { get; }
    IScript CreateScript();
}
