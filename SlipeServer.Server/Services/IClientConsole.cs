using SlipeServer.Server.Elements;

namespace SlipeServer.Server.Services;

public interface IClientConsole
{
    void Output(string message);
    void OutputTo(Player player, string message);
}