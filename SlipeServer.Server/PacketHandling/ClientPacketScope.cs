using System;
using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling;

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

    public void Dispose()
    {
        AsyncLocalScopeStack.Instance.Pop();
        GC.SuppressFinalize(this);
    }

    public bool ContainsClient(IClient client) => this.clients.Contains(client);
}
