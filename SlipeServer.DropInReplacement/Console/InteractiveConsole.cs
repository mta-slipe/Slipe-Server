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

    private Task? runTask;
    private Task? inputTask;

    private readonly ConcurrentQueue<string> queuedOutputs = [];

    private readonly Lock writeLock = new();

    public InteractiveConsole(IMtaServer server, Configuration configuration, Lazy<ConsoleCommandHandler> consoleCommandHandler)
    {
        this.server = server;
        this.configuration = configuration;
        this.consoleCommandHandler = consoleCommandHandler;

        server.Started += Initialise;
    }

    private async Task RunTask()
    {
        while (true)
        {
            lock (this.writeLock)
            {
                while (this.queuedOutputs.TryDequeue(out var line))
                {
                    WriteNewLine(line);
                }

                WriteHeader();
                WriteConsoleInput();
            }

            await Task.Delay(50);
        }
    }

    private async Task InputTask()
    {
        while (true)
        {
            lock (this.writeLock)
            {
                while (System.Console.KeyAvailable)
                {
                    var key = System.Console.ReadKey(false);
                    if (key.Key == ConsoleKey.Enter)
                        SubmitInput();
                    else if (key.Key == ConsoleKey.Backspace)
                        this.input = this.input.Length > 0 ? this.input.Substring(0, this.input.Length - 1) : "";
                    else if (key.KeyChar != '\u0000')
                        this.input += key.KeyChar;
                }
            }

            await Task.Delay(10);
        }
    }

    private void SubmitInput()
    {
        var input = this.input;
        this.input = "";
        this.WriteLine(input);

        try
        {
            this.consoleCommandHandler.Value.Handle(input);
        } catch (Exception e)
        {
            this.WriteLine($"Failed to handle {input}.\n{e.Message}");
        }

    }

    private void Initialise(IMtaServer obj)
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

        this.runTask = RunTask();
        this.inputTask = InputTask();
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
        System.Console.BackgroundColor = ConsoleColor.Gray;
        System.Console.ForegroundColor = ConsoleColor.DarkBlue;

        System.Console.CursorLeft = 0;
        System.Console.CursorTop = 0;
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

        var spaceCount = System.Console.WindowWidth - System.Console.CursorLeft;
        System.Console.Write($"{new string(' ', spaceCount)}");


        System.Console.ResetColor();
    }

    private void WriteConsoleInput()
    {
        System.Console.ResetColor();

        System.Console.CursorTop = Math.Min(this.lineCount, System.Console.WindowHeight - 1);
        System.Console.CursorLeft = 0;

        var spaceCount = System.Console.WindowWidth - this.input.Length;

        System.Console.ForegroundColor = ConsoleColor.DarkYellow;
        System.Console.Write($"{this.input}{new string(' ', spaceCount)}");
        System.Console.CursorLeft = this.input.Length;
    }
}
