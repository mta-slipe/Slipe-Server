namespace SlipeServer.Server.Concepts;

public struct TextItem
{
    public ulong Id { get; init; }

    public TextItem(ulong id)
    {
        this.Id = id;
    }
}
