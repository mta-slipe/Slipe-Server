using System;

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

        public delegate void ConsoleOutputHandler(string message);
        public event ConsoleOutputHandler ConsoleOutput;

        public void Output(string message)
        {
            ConsoleOutput(message);
        }

        public void Output(string message, params object[] vs)
        {
            ConsoleOutput(string.Format(message, vs));
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
