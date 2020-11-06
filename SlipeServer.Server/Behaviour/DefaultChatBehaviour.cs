using System;
using System.Drawing;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.Behaviour
{
    /// <summary>
    /// Behaviour responsible for sending chat echo packets to mimic default MTA main chat
    /// </summary>
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
