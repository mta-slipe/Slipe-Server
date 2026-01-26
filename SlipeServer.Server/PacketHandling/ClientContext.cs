using SlipeServer.Server.Clients;
using System.Threading;

namespace SlipeServer.Server.PacketHandling;

public static class ClientContext
{
    private static AsyncLocal<IClient?> CurrentClient = new();
    public static IClient? Current { get => CurrentClient.Value; set => CurrentClient.Value = value; }
}
