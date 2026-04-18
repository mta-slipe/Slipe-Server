using SlipeServer.Server;
using System.Collections.Concurrent;

namespace SlipeServer.DropInReplacement.Console;

public class InteractiveConsole
{
    private readonly IMtaServer server;
    private readonly Configuration configuration;
    private readonly Lazy<ConsoleCommandHandler> consoleCommandHandler;
    private string input = "";

    private int lineCount;

    private int spinnerIndex = 0;
    private readonly char[] spinner = ['/', '-', '\\', '|', '/', '-', '\\', '|'];

    private readonly Task runTask;
    private readonly Task inputTask;

    private readonly ConcurrentQueue<string> queuedOutputs = [];

    public InteractiveConsole(IMtaServer server, Configuration configuration, Lazy<ConsoleCommandHandler> consoleCommandHandler)
    {
        server.Started += OutputInitial;
        this.server = server;
        this.configuration = configuration;
        this.consoleCommandHandler = consoleCommandHandler;
        this.runTask = RunTask();
        this.inputTask = InputTask();
    }

    private async Task RunTask()
    {
        while (true)
        {
            while (this.queuedOutputs.TryDequeue(out var line))
            {
                WriteNewLine(line);
            }

            WriteHeader();
            WriteConsoleInput();

            await Task.Delay(50);
        }
    }

    private async Task InputTask()
    {
        while (true)
        {
            while (System.Console.KeyAvailable)
            {
                var key = System.Console.ReadKey(false);
                if (key.Key == ConsoleKey.Enter)
                    SubmitInput();
                else
                    this.input += key.KeyChar;
            }

            await Task.Delay(10);
        }
    }

    private void SubmitInput()
    {
        var input = this.input;
        this.input = "";
        this.WriteLine("");

        try
        {
            this.consoleCommandHandler.Value.Handle(input);
        } catch (Exception e)
        {
            this.WriteLine($"Failed to handle {input}.\n{e.Message}");
        }

    }

    private void OutputInitial(IMtaServer obj)
    {
        System.Console.ResetColor();
        System.Console.Clear();
        System.Console.Title = "Slipe Server Drop In Replacement";
        System.Console.CursorVisible = false;

        WriteNewLine($"""
            Slipe Server Drop In Replacement

            Please wait ...
            =======================================================
            = Slipe Server
            =======================================================
            = Server name      : {configuration.ServerName}
            = Server IP address: {configuration.Host}
            = Server port      : {configuration.Port}
            =
            = Log file         : N/A
            = Maximum players  : {configuration.MaxPlayerCount}
            = HTTP port        : {configuration.HttpPort}
            = Voice Chat       : {(configuration.IsVoiceEnabled ? "Enabled" : "Disabled")}
            = Bandwidth saving : N/A
            =======================================================
            """);
    }

    public void WriteLine(string line)
    {
        System.Console.ResetColor();

        this.queuedOutputs.Enqueue(line);
    }

    private void WriteNewLine(string line)
    {
        System.Console.ResetColor();

        System.Console.CursorLeft = 0;
        System.Console.CursorTop = Math.Min(this.lineCount, System.Console.WindowHeight - 1);
        System.Console.WriteLine(line);

        this.lineCount += line.Count('\n') + 1;
    }

    private void WriteHeader()
    {
        System.Console.CursorLeft = 0;
        System.Console.CursorTop = 0;
        System.Console.BackgroundColor = ConsoleColor.Gray;

        System.Console.ForegroundColor = ConsoleColor.Blue;
        System.Console.Write($"[{this.spinner[++this.spinnerIndex % this.spinner.Length]}]");

        System.Console.ForegroundColor = ConsoleColor.Black;
        System.Console.Write($"Slipe Server ");

        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.Write($" :: ");

        System.Console.ForegroundColor = ConsoleColor.Black;
        System.Console.Write($"{this.server.Players.Count()}/{this.configuration.MaxPlayerCount} players");

        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.Write($" :: ");

        System.Console.ForegroundColor = ConsoleColor.Black;
        System.Console.Write($"{0} Resources");

        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.Write($" :: ");

        System.Console.ForegroundColor = ConsoleColor.Black;
        System.Console.Write($"{0} fps ({0})");

        System.Console.ResetColor();
    }

    private void WriteConsoleInput()
    {
        System.Console.ResetColor();

        System.Console.CursorTop = Math.Min(this.lineCount, System.Console.WindowHeight);
        System.Console.CursorLeft = 0;

        var spaceCount = System.Console.WindowWidth - this.input.Length;

        System.Console.Write($"{this.input}{new string(' ', spaceCount)}");
        System.Console.CursorLeft = this.input.Length;
    }
}
