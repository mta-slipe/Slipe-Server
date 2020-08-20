using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace MtaServer.Server.PacketHandling
{
    public class ClientPacketScope: IDisposable
    {
        private static AsyncLocal<Stack<ClientPacketScope>>? scopeStack;
        public static ClientPacketScope? Current => Peek();


        private bool isCompleted = false;

        private readonly HashSet<Client> clients;

        public ClientPacketScope(IEnumerable<Client> clients)
        {
            this.clients = new HashSet<Client>(clients);

            Push(this);
        }

        public void Dispose()
        {
            if (!isCompleted)
            {
                Pop();
            }
        }

        public void Complete()
        {
            Pop();
            isCompleted = true;
        }

        public bool ContainsClient(Client client) => this.clients.Contains(client);

        private static ClientPacketScope? Peek()
        {
            EnsureScopeStackExists();
            if (scopeStack?.Value == null)
                return null;

            scopeStack.Value.TryPeek(out ClientPacketScope? result);
            return result;
        }

        private static ClientPacketScope? Pop()
        {
            EnsureScopeStackExists();
            return scopeStack?.Value?.Pop();
        }

        private static void Push(ClientPacketScope scope)
        {
            EnsureScopeStackExists();
            scopeStack?.Value?.Push(scope);
        }

        private static void EnsureScopeStackExists()
        {
            if (scopeStack == null)
            {
                scopeStack = new AsyncLocal<Stack<ClientPacketScope>>();
            }
            if (scopeStack.Value == null)
            {
                scopeStack.Value = new Stack<ClientPacketScope>();
            }
        }
    }
}
