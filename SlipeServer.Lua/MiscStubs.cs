namespace SlipeServer.Lua;

public static class MiscStubs
{
    public const string TableGetN = """
        function table.getn(t)
            return #t
        end
        """;

    public const string All = $"""
        {TableGetN}
    """;
}
