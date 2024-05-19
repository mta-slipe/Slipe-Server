using System.Threading;

namespace SlipeServer.Scripting;

public static class ServerResourceContext
{
    private readonly static AsyncLocal<ServerResource?> _current = new AsyncLocal<ServerResource?>();

    public static ServerResource? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}
