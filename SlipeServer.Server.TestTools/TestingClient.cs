using SlipeServer.Net.Wrappers;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.TestTools;

public class TestingClient : Client
{
    public Player TestingPlayer
    {
        get => this.Player as TestingPlayer;
        set => this.Player = value;
    }

    public TestingClient(uint address, INetWrapper netWrapper) : base(address, netWrapper)
    {
        this.ConnectionState = Enums.ClientConnectionState.Joined;
    }
}
