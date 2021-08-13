using System.IO;

namespace SlipeServer.Server.Elements
{
    public class PlayerPendingScreenshot
    {
        public MemoryStream? Stream { get; init; }
        public string? ErrorMessage { get; init; }
        public string Tag { get; init; } = "";
        public uint TotalParts { get; init; }
    }
}
