using SlipeServer.Net.Wrappers;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.TestTools;

public class TestingClient : Client<TestingPlayer>
{
    public uint Address { get; private set; }

    public TestingClient(uint address, INetWrapper netWrapper, TestingPlayer player)
        : base(address, netWrapper, player)
    {
        this.Address = address;
        this.ConnectionState = Enums.ClientConnectionState.Joined;
    }
}
