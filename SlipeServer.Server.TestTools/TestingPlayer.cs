using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.TestTools;

public class TestingPlayer : Player
{
    public static Player CreateStandalone()
    {
        var netWrapper = new Mock<INetWrapper>();
        var player = new TestingPlayer();
        player.Client = new TestingClient(0, netWrapper.Object, player);
        return player;
    }
}
