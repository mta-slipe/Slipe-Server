using System;
using System.IO;

namespace SlipeServer.Server.Elements.Events;

public class ScreenshotEventArgs : EventArgs
{
    public Stream Stream { get; }
    public string? ErrorMessage { get; }
    public string Tag { get; set; }

    public ScreenshotEventArgs(Stream stream, string? errorMessage, string tag)
    {
        this.Stream = stream;
        this.ErrorMessage = errorMessage;
        this.Tag = tag;
    }
}
