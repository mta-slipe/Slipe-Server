using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Console
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
            System.Console.WriteLine($"{this.prefix}{formatter(state, exception)}");
        }
    }
}
