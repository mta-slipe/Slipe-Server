using Microsoft.Extensions.Logging;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Services;


namespace SlipeServer.Server.Loggers;

[ProviderAlias("ConsoleLogger")]
public sealed class ConsoleLoggerProvider(
    IElementCollection elementCollection,
    IDebugLog debugLog) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new ConsoleLogger(elementCollection, debugLog);

    public void Dispose()
    {

    }
}
