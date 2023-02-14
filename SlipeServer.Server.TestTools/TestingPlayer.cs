using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.TestTools;

public class TestingPlayer : Player
{
    public uint Address => ((TestingClient)this.Client).Address;

    public TestingPlayer() : base()
    {

    }

    public new TestingPlayer AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public static TestingPlayer CreateStandalone()
    {
        var netWrapper = new Mock<INetWrapper>();
        var player = new TestingPlayer();
        player.Client = new TestingClient(0, netWrapper.Object, player);
        return player;
    }
}
