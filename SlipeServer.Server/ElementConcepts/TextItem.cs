namespace SlipeServer.Server.Concepts;

/// <summary>
/// Represents a 2d text item displayed on the client's screen.
/// </summary>
public readonly struct TextItem(ulong id)
{
    public ulong Id { get; init; } = id;
}
