using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Console.Commands
{
    public interface ICommand
    {
        public string Name { get; }
        public string Description { get; }
        public string Usage { get; }
        public bool Execute(Program program, IEnumerable<string> args, out string errorMessage);
    }
}
