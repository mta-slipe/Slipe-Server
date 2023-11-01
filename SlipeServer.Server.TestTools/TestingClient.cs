using SlipeServer.Net.Wrappers;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.TestTools;

public class TestingClient : Client<Player>
{
    public uint Address { get; private set; }

    public TestingClient(uint address, INetWrapper netWrapper, Player player)
        : base(address, netWrapper, player)
    {
        this.Address = address;
        this.ConnectionState = Enums.ClientConnectionState.Joined;
    }
}
