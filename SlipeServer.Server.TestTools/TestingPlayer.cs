using SlipeServer.Server.Elements;
using System;

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
}
