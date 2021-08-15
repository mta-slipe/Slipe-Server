namespace SlipeServer.Server.ElementConcepts
{
    public struct TextItem
    {
        public ulong Id { get; init; }

        public TextItem(ulong id)
        {
            this.Id = id;
        }
    }
}
