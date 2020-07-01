using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtaServer.Console.Commands
{
    public class HelpCommand : ICommand
    {
        public string GetName() => "help";
        public string GetDescription() => "Displaying all command or help about specific command.";
        public string GetUsage() => "help [command name]";

        public bool Execute(Program program, IEnumerable<string> args, out string errorMessage)
        {
            int argsCount = args.Count();
            if (argsCount == 0)
            {
                System.Console.WriteLine($"Commands: {string.Join(", ",program.Commands.GetCommands())}");
                errorMessage = "";
                return true;
            }
            else if(argsCount == 1)
            {
                string targetCommand = args.ElementAt(0);
                ICommand command = program.Commands.GetCommandByName(targetCommand);
                if(command == null)
                {
                    errorMessage = $"Command '{targetCommand}' not found.";
                    return false;
                }
                System.Console.WriteLine($"{targetCommand}:\r\n\tDescription: {command.GetDescription()}\r\n\tHelp: {command.GetUsage()}");
                errorMessage = "";
                return true;
            }
            errorMessage = "Too many arguments.";
            return false;
        }
    }
}
