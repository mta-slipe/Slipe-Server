using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Server.Services;

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
