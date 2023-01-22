using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling;

/// <summary>
/// Scope that allows you to specify a set of players to send packets to.
/// More information can be found at https://server.mta-slipe.com/articles/advanced-features/client-packet-scope.html
/// </summary>
public class ClientPacketScope : IDisposable
{
    public static ClientPacketScope? Current => AsyncLocalScopeStack.Instance.Peek();

    private readonly HashSet<IClient> clients;

    public ClientPacketScope(HashSet<IClient> clients)
    {
        this.clients = clients;

        AsyncLocalScopeStack.Instance.Push(this);
    }

    public ClientPacketScope(IEnumerable<IClient> clients) : this(new HashSet<IClient>(clients))
    {
    }

    public ClientPacketScope(IClient client) : this(new HashSet<IClient>() { client })
    {
    }

    public ClientPacketScope(Player player) : this(player.Client)
    {
    }

    public ClientPacketScope(IEnumerable<Player> players) : this(players.Select(x => x.Client))
    {
    }

    public void Dispose()
    {
        AsyncLocalScopeStack.Instance.Pop();
        GC.SuppressFinalize(this);
    }

    public bool ContainsClient(IClient client) => this.clients.Contains(client);
}
