using System;

namespace MtaServer.Console.Logic
{
    public class ConsoleInputArgs : EventArgs
    {
        public string Line { set; get; }
    }

    public class ConsoleHandler
    {
        public void Output(string message)
        {
            ConsoleOutput?.Invoke(message);
        }

        public void Output(string message, params object[] vs)
        {
            ConsoleOutput?.Invoke(string.Format(message, vs));
        }

        public void HandleConsoleInput(string line)
        {
            if(!string.IsNullOrEmpty(line))
            {
                ConsoleInput?.Invoke(new ConsoleInputArgs
                {
                    Line = line
                });
            }
        }

        public ConsoleHandler()
        {

        }

        public delegate void ConsoleInputHandler(ConsoleInputArgs args);
        public event ConsoleInputHandler ConsoleInput;
        public delegate void ConsoleOutputHandler(string message);
        public event ConsoleOutputHandler ConsoleOutput;
    }
}
