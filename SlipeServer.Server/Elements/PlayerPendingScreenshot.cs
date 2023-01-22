using System.IO;

namespace SlipeServer.Server.Elements;

/// <summary>
/// A server requested screen capture of the player's screen
/// </summary>
public class PlayerPendingScreenshot
{
    public MemoryStream? Stream { get; init; }
    public string? ErrorMessage { get; init; }
    public string Tag { get; init; } = "";
    public uint TotalParts { get; init; }
}
