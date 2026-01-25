using SlipeServer.Server.Concepts;

namespace SlipeServer.Server.Services;

public interface ICommandService
{
    Command AddCommand(string command, bool isCaseSensitive = true);
    void RemoveCommand(Command command);
}