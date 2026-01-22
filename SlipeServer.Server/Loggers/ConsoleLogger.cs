using Microsoft.Extensions.Logging;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlipeServer.Server.Loggers;

/// <summary>
/// ILogger implementation which logs messages to the console
/// </summary>
public class ConsoleLogger : ILogger
{
    private class ConsoleLoggerScope : IDisposable
    {
        private readonly Action onComplete;

        public ConsoleLoggerScope(Action onComplete)
        {
            this.onComplete = onComplete;
        }

        public void Dispose()
        {
            this.onComplete();
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
    private readonly DebugLog debugLog;
    private string prefix;

    private readonly ConcurrentQueue<Action> logActions;

    public ConsoleLogger(IElementCollection elementCollection, DebugLog debugLog)
    {
        this.elementCollection = elementCollection;
        this.debugLog = debugLog;

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

        return new ConsoleLoggerScope(() => this.prefix = this.prefix[^2..]);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
#if !DEBUG
            if (logLevel == LogLevel.Trace)
                return;
#endif

        var prefix = this.prefix;
        this.logActions.Enqueue(() =>
        {
            System.Console.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]");

            System.Console.ForegroundColor = prefixes[logLevel].Item1;
            System.Console.Write($"{prefixes[logLevel].Item2}");
            System.Console.ResetColor();

            System.Console.WriteLine($" {prefix}{formatter(state, exception)}");

            if (exception != null)
            {
                System.Console.WriteLine($" {prefix}{exception.StackTrace}");
            }

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
