using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Console
{
    class ConsoleLoggerScope: IDisposable
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

    public class ConsoleLogger : ILogger
    {
        private static Dictionary<LogLevel, Tuple<ConsoleColor, string>> prefixes = new Dictionary<LogLevel, Tuple<ConsoleColor, string>>()
        {
            [LogLevel.Trace] = new Tuple<ConsoleColor, string>(ConsoleColor.Gray, " [trace]   "),
            [LogLevel.Debug] = new Tuple<ConsoleColor, string>(ConsoleColor.Yellow, " [debug]   "),
            [LogLevel.Information] = new Tuple<ConsoleColor, string>(ConsoleColor.Blue, " [info]    "),
            [LogLevel.Warning] = new Tuple<ConsoleColor, string>(ConsoleColor.DarkYellow, " [warn]    "),
            [LogLevel.Error] = new Tuple<ConsoleColor, string>(ConsoleColor.Red, " [error]   "),
            [LogLevel.Critical] = new Tuple<ConsoleColor, string>(ConsoleColor.DarkRed, " [critical]"),
            [LogLevel.None] = new Tuple<ConsoleColor, string>(ConsoleColor.White, "        "),
        };

        private string prefix;

        public ConsoleLogger()
        {
            this.prefix = "";
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            prefix += "  ";
            System.Console.WriteLine($"{this.prefix}{state}:");

            return new ConsoleLoggerScope(() => this.prefix = this.prefix.Substring(this.prefix.Length - 2));
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            #if !DEBUG
            if (logLevel == LogLevel.Trace)
                return;
            #endif

            System.Console.Write($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}]");

            System.Console.ForegroundColor = prefixes[logLevel].Item1;
            System.Console.Write($"{prefixes[logLevel].Item2}");
            System.Console.ResetColor();

            System.Console.WriteLine($" {this.prefix}{formatter(state, exception)}");

            if (exception != null)
            {
                System.Console.WriteLine($" {this.prefix}{exception.StackTrace}");
            }
        }
    }
}
