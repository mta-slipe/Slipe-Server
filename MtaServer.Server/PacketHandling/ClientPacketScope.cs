using System;
using System.Collections.Generic;

namespace MtaServer.Server.PacketHandling
{
    public class ClientPacketScope: IDisposable
    {
        public static ClientPacketScope? Current => AsyncLocalScopeStack.Instance.Peek();

        private readonly HashSet<Client> clients;

        public ClientPacketScope(HashSet<Client> clients)
        {
            this.clients = clients;

            AsyncLocalScopeStack.Instance.Push(this);
        }

        public ClientPacketScope(IEnumerable<Client> clients) : this(new HashSet<Client>(clients))
        {
        }

        public void Dispose() => AsyncLocalScopeStack.Instance.Pop();

        public bool ContainsClient(Client client) => this.clients.Contains(client);
    }
}
