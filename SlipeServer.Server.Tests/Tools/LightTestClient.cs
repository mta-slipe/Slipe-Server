using SlipeServer.Net.Wrappers;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.Tests.Tools;

public class LightTestClient : Client<Player>
{
    public ulong Address { get; private set; }

    public LightTestClient(ulong address, INetWrapper netWrapper, Player player)
        : base(address, netWrapper, player)
    {
        this.Address = address;
        this.ConnectionState = Enums.ClientConnectionState.Joined;
    }
}
