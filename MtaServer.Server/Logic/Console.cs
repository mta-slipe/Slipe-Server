using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Logic
{

    public class ConsoleInputArgs : EventArgs
    {
        public string Line { set; get; }
    }

    public class Console
    {
        public delegate void ConsoleInputHandler(ConsoleInputArgs args);
        public event ConsoleInputHandler ConsoleHandler;

        public bool Output(string message)
        {
            System.Console.WriteLine(message);
            return true;
        }

        public bool Output(string message, params object[] vs)
        {
            System.Console.WriteLine(message, vs);
            return true;
        }

        public void HandleConsoleInput(string line)
        {
            if(!string.IsNullOrEmpty(line))
            {
                ConsoleHandler(new ConsoleInputArgs
                {
                    Line = line
                });
            }
        }

        public Console()
        {

        }
    }
}
