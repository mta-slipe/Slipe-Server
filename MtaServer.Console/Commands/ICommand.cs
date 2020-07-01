using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Console.Commands
{
    public interface ICommand
    {
        public string GetName();
        public string GetDescription();
        public string GetUsage();
        public bool Execute(Program program, IEnumerable<string> args, out string errorMessage);
    }
}
