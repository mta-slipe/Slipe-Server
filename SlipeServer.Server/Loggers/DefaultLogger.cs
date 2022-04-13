using Microsoft.Extensions.Logging;
using System;

namespace SlipeServer.Server.Loggers;

class DefaultLoggerScope : IDisposable
{
    public void Dispose() { }
}

class DefaultLogger : ILogger
{
    private readonly IDisposable disposable = new DefaultLoggerScope();

    public IDisposable BeginScope<TState>(TState state) => disposable;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {

    }
}
