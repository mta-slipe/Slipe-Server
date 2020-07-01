using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtaServer.Console.Commands
{
    public class Commands
    {
        internal Program Program { get; }
        private Dictionary<string, ICommand> CommandsCache { get; } = new Dictionary<string, ICommand>();

        public Commands(Program program)
        {
            this.Program = program;
            CacheCommands();
        }

        public IEnumerable<string> GetCommands() => CommandsCache.Keys;

        public void CacheCommands()
        {
            var type = typeof(ICommand);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

            foreach (Type commandType in types)
            {
                ICommand? command = Activator.CreateInstance(commandType) as ICommand;
                if (command == null)
                {
                    throw new Exception($"Unable to create command from {commandType} class.");
                }

                CommandsCache.Add(command.GetName(), command);
            }
        }

        public ICommand? GetCommandByName(string command)
        {
            if (CommandsCache.TryGetValue(command, out ICommand value))
            {
                return value;
            }
            return null;
        }

        public bool ExecuteCommand(string line)
        {
            string[] args = line.Split(' ');
            if(CommandsCache.TryGetValue(args[0], out ICommand value))
            {
                string errorMessage;
                if(!value.Execute(Program, args.Skip(1), out errorMessage))
                {
                    System.Console.WriteLine($"Could not execute command '{args[0]}': {errorMessage}");
                }
                return true;
            }
            return false;
        }
    }
}
