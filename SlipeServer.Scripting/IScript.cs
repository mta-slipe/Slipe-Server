namespace SlipeServer.Scripting;

public interface IScript
{
    void LoadCode(byte[] code, string chunkName);
}
