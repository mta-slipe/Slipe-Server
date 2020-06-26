using System;
using System.Collections.Generic;
using System.Linq;

namespace MtaServer.Server.Logic
{
    public class Commands
    {
        internal Dictionary<string, Action<IEnumerable<string>>> RegisteredCommands = new Dictionary<string, Action<IEnumerable<string>>>();
        internal MtaServer MtaServer { get; }

        private void RegisterCommand(string command, Action<IEnumerable<string>> action)
        {
            RegisteredCommands[command] = action;
        }

        private void Quit(IEnumerable<string> arguments)
        {
            MtaServer.Shutdown();
        }

        private void Help(IEnumerable<string> arguments)
        {
            MtaServer.Console.Output("Commands: {0}", string.Join(", ", RegisteredCommands.Keys));
        }

        public Commands(MtaServer mtaServer)
        {
            MtaServer = mtaServer;

            MtaServer.Console.ConsoleInput += ConsoleConsoleHandler;

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
                MtaServer.Console.Output("Command '{0}' not found. Type 'help' for help", command);
            }
        }
    }
}
