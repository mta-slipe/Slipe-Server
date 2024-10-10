using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Abstractions;

namespace SlipeServer.Legacy;

public class CustomConsoleFormatter : ConsoleFormatter
{
    private readonly SimpleConsoleFormatterOptions formatterOptions;

    public CustomConsoleFormatter(IOptionsMonitor<SimpleConsoleFormatterOptions> options)
        : base("customFormatter")
    {
        this.formatterOptions = new SimpleConsoleFormatterOptions
        {
            TimestampFormat = "[HH:mm:ss] "
        };
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        if (textWriter == null)
        {
            throw new ArgumentNullException(nameof(textWriter));
        }

        var timestamp = DateTime.Now.ToString(this.formatterOptions.TimestampFormat);
        textWriter.Write(timestamp);
        textWriter.Write(logEntry.Formatter(logEntry.State, logEntry.Exception));
        textWriter.Write(Environment.NewLine);
    }
}
