using System;
using System.Collections.Generic;
using System.Linq;

namespace MtaServer.Console.Logic
{
    public class Commands
    {
        internal Dictionary<string, Action<IEnumerable<string>>> RegisteredCommands = new Dictionary<string, Action<IEnumerable<string>>>();
        internal Server.MtaServer MtaServer { get; }
        internal ConsoleHandler ConsoleHandler { get; }

        private void RegisterCommand(string command, Action<IEnumerable<string>> action)
        {
            RegisteredCommands[command] = action;
        }

        private void Quit(IEnumerable<string> arguments)
        {
            ConsoleHandler.Output("Server is shutting down...");
            MtaServer.Shutdown();
        }

        private void Help(IEnumerable<string> arguments)
        {
            ConsoleHandler.Output("Commands: {0}", string.Join(", ", RegisteredCommands.Keys));
        }

        public Commands(Server.MtaServer mtaServer, ConsoleHandler consoleHandler)
        {
            MtaServer = mtaServer;
            ConsoleHandler = consoleHandler;

            ConsoleHandler.ConsoleInput += ConsoleConsoleHandler;

            RegisterCommand("help", Help);
            RegisterCommand("quit", Quit);
            RegisterCommand("shutdown", Quit);
        }

        private void ConsoleConsoleHandler(ConsoleInputArgs args)
        {
            IEnumerable<string> arguments = args.Line.Split(" ");

            string command = arguments.ElementAt(0);
            if (RegisteredCommands.TryGetValue(command, out Action<IEnumerable<string>> action))
            {
                action.Invoke(arguments.Skip(1));
            }
            else
            {
                ConsoleHandler.Output("Command '{0}' not found. Type 'help' for help", command);
            }
        }
    }
}
