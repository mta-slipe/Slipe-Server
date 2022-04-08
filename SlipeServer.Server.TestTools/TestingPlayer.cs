using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.TestTools;

public class TestingPlayer : Player
{
    public uint Address { get; }

    public TestingPlayer(Client client, uint address) : base(client)
    {
        this.Address = address;
    }

    public new TestingPlayer AssociateWith(MtaServer server)
    {
        return server.AssociateElement(this);
    }

    public static TestingPlayer CreateStandalone()
    {
        var netWrapper = new Mock<INetWrapper>();
        return new TestingPlayer(new TestingClient(0, netWrapper.Object), 0);
    }
}
