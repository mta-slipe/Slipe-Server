using System;
using System.Drawing;
using Microsoft.Extensions.Logging;
using MtaServer.Packets.Definitions.Commands;
using MtaServer.Server.Elements;

namespace MtaServer.Server.Behaviour
{
    public class DefaultChatBehaviour
    {
        public DefaultChatBehaviour(MtaServer server, ILogger? logger)
        {
            server.PlayerJoined += (player) =>
            {
                player.OnCommand += (command, arguments) =>
                {
                    if(command == "say")
                    {
                        string message = $"{player.Name}: {string.Join(' ', arguments)}";
                        var packet = new ChatEchoPacket(player.Id, message, Color.White);
                        server.BroadcastPacket(packet);
                        logger?.LogInformation(message);
                    }
                };
            };
        }
    }
}
