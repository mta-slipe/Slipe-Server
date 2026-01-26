using System;
using System.IO;

namespace SlipeServer.Server.Elements.Events;

public sealed class ScreenshotEventArgs(Stream stream, string? errorMessage, string tag) : EventArgs
{
    public Stream Stream { get; } = stream;
    public string? ErrorMessage { get; } = errorMessage;
    public string Tag { get; } = tag;
}
