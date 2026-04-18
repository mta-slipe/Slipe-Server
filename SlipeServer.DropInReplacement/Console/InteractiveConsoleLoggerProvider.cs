using Microsoft.Extensions.Logging;
using SlipeServer.Server;


namespace SlipeServer.DropInReplacement.Console;

[ProviderAlias("InteractiveConsoleLogger")]
public sealed class InteractiveConsoleLoggerProvider(
    IMtaServer mtaServer) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => mtaServer.Instantiate<InteractiveConsoleLogger>();

    public void Dispose()
    {

    }
}
