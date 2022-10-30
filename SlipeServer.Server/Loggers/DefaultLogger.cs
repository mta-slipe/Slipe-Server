using Microsoft.Extensions.Logging;
using System;

namespace SlipeServer.Server.Loggers;

public class DefaultLoggerScope : IDisposable
{
    public void Dispose() {
        GC.SuppressFinalize(this);
    }
}

public class DefaultLogger : ILogger
{
    private readonly IDisposable disposable = new DefaultLoggerScope();

    public IDisposable BeginScope<TState>(TState state) => this.disposable;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {

    }
}
