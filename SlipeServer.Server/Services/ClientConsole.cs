using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.Services;

/// <summary>
/// Represents the F8 console on clients. 
/// </summary>
public class ClientConsole
{
    private readonly MtaServer server;

    public ClientConsole(MtaServer server)
    {
        this.server = server;
    }

    public void Output(string message)
    {
        this.server.BroadcastPacket(new ConsoleEchoPacket(message));
    }

    public void OutputTo(Player player, string message)
    {
        player.Client.SendPacket(new ConsoleEchoPacket(message));
    }
}
