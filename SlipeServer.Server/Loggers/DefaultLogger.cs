using Microsoft.Extensions.Logging;
using System;

namespace SlipeServer.Server.Loggers;

public class NullLoggerScope : IDisposable
{
    public void Dispose() {
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// ILogger implementation that does not log messages
/// </summary>
public class NullLogger : ILogger
{
    private readonly IDisposable disposable = new NullLoggerScope();

    public IDisposable BeginScope<TState>(TState state) => this.disposable;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {

    }
}
