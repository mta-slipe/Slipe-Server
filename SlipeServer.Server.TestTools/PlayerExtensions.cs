using SlipeServer.Server.Elements;

namespace SlipeServer.Server.TestTools;

public static class PlayerExtensions
{
    public static ulong GetAddress(this Player player) => ((TestingClient)player.Client).Address;
}
