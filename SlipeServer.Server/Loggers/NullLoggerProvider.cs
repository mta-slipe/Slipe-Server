using Microsoft.Extensions.Logging;


namespace SlipeServer.Server.Loggers;

[ProviderAlias("NullLogger")]
public sealed class NullLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new NullLogger();

    public void Dispose()
    {

    }
}
