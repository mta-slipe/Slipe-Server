namespace MtaServer.Console
{
    public class ConsoleHandler
    {
        internal Program Program { get; }
        public void HandleConsoleInput(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if(Program.Commands.ExecuteCommand(line))
                {
                    return;
                }
                string command = line.Split(' ')[0];
                System.Console.WriteLine($"Command '{command}' not found, type 'help' to see availiable commands.");
            }
        }

        public ConsoleHandler(Program program)
        {
            this.Program = program;
        }
    }
}
