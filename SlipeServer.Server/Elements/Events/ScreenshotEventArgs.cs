using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public class ScreenshotEventArgs : EventArgs
    {
        public Stream Stream { get; }
        public string? ErrorMessage { get; }

        public ScreenshotEventArgs(Stream stream, string? errorMessage)
        {
            Stream = stream;
            ErrorMessage = errorMessage;
        }
    }
}
