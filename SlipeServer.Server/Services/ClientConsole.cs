using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.Services;

/// <summary>
/// Represents the F8 console on clients. 
/// </summary>
public class ClientConsole(MtaServer server) : IClientConsole
{
    public void Output(string message)
    {
        server.BroadcastPacket(new ConsoleEchoPacket(message));
    }

    public void OutputTo(Player player, string message)
    {
        player.Client.SendPacket(new ConsoleEchoPacket(message));
    }
}
