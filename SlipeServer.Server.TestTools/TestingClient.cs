using SlipeServer.Net.Wrappers;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.TestTools;

public class TestingClient : Client<Player>
{
    public ulong Address { get; private set; }

    public TestingClient(ulong address, INetWrapper netWrapper, Player player)
        : base(address, netWrapper, player)
    {
        this.Address = address;
        this.ConnectionState = Enums.ClientConnectionState.Joined;
    }
}
