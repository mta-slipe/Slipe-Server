using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace MtaServer.Server.PacketHandling
{
    public class AsyncLocalScopeStack
    {
        internal static AsyncLocalScopeStack Instance { get; } = new AsyncLocalScopeStack();

        private static readonly AsyncLocal<Stack<ClientPacketScope>> scopeStack = new AsyncLocal<Stack<ClientPacketScope>>();

        public AsyncLocalScopeStack()
        {

        }

        public void Push(ClientPacketScope scope)
        {
            scopeStack.Value ??= new Stack<ClientPacketScope>();
            scopeStack.Value.Push(scope);
        }

        public ClientPacketScope? Peek()
        {
            if (scopeStack.Value == null)
            {
                return null;
            }
            scopeStack.Value ??= new Stack<ClientPacketScope>();
            scopeStack.Value.TryPeek(out var result);
            return result;
        }

        public ClientPacketScope? Pop()
        {
            scopeStack.Value ??= new Stack<ClientPacketScope>();
            scopeStack.Value.TryPop(out var result);
            return result;
        }
    }
}
