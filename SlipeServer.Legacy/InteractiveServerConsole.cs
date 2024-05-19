namespace SlipeServer.Legacy;

public delegate void ConsoleCommandCallback(string arguments);

public class InteractiveServerConsole
{
    private readonly Dictionary<string, ConsoleCommandCallback> commands = [];

    public InteractiveServerConsole()
    {

    }

    public bool AddCommand(string command, ConsoleCommandCallback callback)
    {
        if (this.commands.ContainsKey(command))
            return false;

        this.commands[command] = callback;
        return true;
    }

    public void ExecuteCommand(string command)
    {
        var splt = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        var commandName = splt[0];
        if (this.commands.TryGetValue(commandName, out var callback)){

            if (splt.Length == 2)
                callback(splt[1]);
            else
                callback("");
        }
    }
}
