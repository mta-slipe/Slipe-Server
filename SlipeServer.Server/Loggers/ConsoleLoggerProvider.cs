using Microsoft.Extensions.Logging;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Services;


namespace SlipeServer.Server.Loggers;

[ProviderAlias("ConsoleLogger")]
public sealed class ConsoleLoggerProvider : ILoggerProvider
{
    private readonly IElementCollection elementCollection;
    private readonly DebugLog debugLog;

    public ConsoleLoggerProvider(
        IElementCollection elementCollection,
        DebugLog debugLog)
    {
        this.elementCollection = elementCollection;
        this.debugLog = debugLog;
    }

    public ILogger CreateLogger(string categoryName) => new ConsoleLogger(this.elementCollection, this.debugLog);

    public void Dispose()
    {

    }
}
