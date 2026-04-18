using Microsoft.Extensions.Logging;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System.Collections.Concurrent;

namespace SlipeServer.DropInReplacement.Console;

public class InteractiveConsoleLogger : ILogger
{
    private class InteractiveConsoleLoggerScope(Action onComplete) : IDisposable
    {
        public void Dispose()
        {
            onComplete();
        }
    }

    private readonly static Dictionary<LogLevel, Tuple<ConsoleColor, string>> prefixes = new()
    {
        [LogLevel.Trace] = new Tuple<ConsoleColor, string>(ConsoleColor.Gray, " [trace]   "),
        [LogLevel.Debug] = new Tuple<ConsoleColor, string>(ConsoleColor.Yellow, " [debug]   "),
        [LogLevel.Information] = new Tuple<ConsoleColor, string>(ConsoleColor.Blue, " [info]    "),
        [LogLevel.Warning] = new Tuple<ConsoleColor, string>(ConsoleColor.DarkYellow, " [warn]    "),
        [LogLevel.Error] = new Tuple<ConsoleColor, string>(ConsoleColor.Red, " [error]   "),
        [LogLevel.Critical] = new Tuple<ConsoleColor, string>(ConsoleColor.DarkRed, " [critical]"),
        [LogLevel.None] = new Tuple<ConsoleColor, string>(ConsoleColor.White, "        "),
    };

    private readonly IElementCollection elementCollection;
    private readonly IDebugLog debugLog;
    private readonly InteractiveConsole interactiveConsole;
    private string prefix;

    private readonly ConcurrentQueue<Action> logActions;

    public InteractiveConsoleLogger(IElementCollection elementCollection, IDebugLog debugLog, InteractiveConsole interactiveConsole)
    {
        this.elementCollection = elementCollection;
        this.debugLog = debugLog;
        this.interactiveConsole = interactiveConsole;
        this.logActions = new();
        this.prefix = "";

        Task.Run(LogWorker);
    }

    private async Task LogWorker()
    {
        while (true)
        {
            while (this.logActions.TryDequeue(out var action))
            {
                action();
                action = null;
            }
            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        this.prefix += "  ";
        System.Console.WriteLine($"{this.prefix}{state}:");

        return new InteractiveConsoleLoggerScope(() => this.prefix = this.prefix[^2..]);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var prefix = this.prefix;
        this.logActions.Enqueue(() =>
        {
            var message = $"[{DateTime.Now:HH:mm:ss}] {prefixes[logLevel].Item2} {prefix}{formatter(state, exception)}";
            this.interactiveConsole.WriteLine(message);

            if (exception != null)
                this.interactiveConsole.WriteLine($" {prefix}{exception.StackTrace}");

            switch (logLevel)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    OutputDebug($"{prefixes[logLevel].Item2} {prefix}{formatter(state, exception)}", DebugLevel.Error);
                    break;
                case LogLevel.Warning:
                    OutputDebug($"{prefixes[logLevel].Item2} {prefix}{formatter(state, exception)}", DebugLevel.Warning);
                    break;
                case LogLevel.Information:
                    OutputDebug($"{prefixes[logLevel].Item2} {prefix}{formatter(state, exception)}", DebugLevel.Information);
                    break;
            }
        });
    }

    private void OutputDebug(string message, DebugLevel level)
    {
        var players = this.elementCollection.GetByType<Player>(ElementType.Player);
        foreach (var player in players)
        {
            this.debugLog.OutputTo(player, message, level);
        }
    }
}
