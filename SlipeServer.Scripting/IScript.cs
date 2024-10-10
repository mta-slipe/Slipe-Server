namespace SlipeServer.Scripting;

public interface IScript
{
    string Language { get; }
    void LoadCode(byte[] code, string chunkName);
    void SetGlobal(string name, object? value);
    object? GetGlobal(string name);
}
