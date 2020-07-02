using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Microsoft.Extensions.Logging;
using MtaServer.Packets.Definitions.Commands;
using MtaServer.Server.Elements;
using MtaServer.Server.Repositories;

namespace MtaServer.Server.Behaviour
{
    public class DefaultChatBehaviour
    {
        public DefaultChatBehaviour(IElementRepository elementRepository, ILogger? logger)
        {
            Player.OnJoin += (player) =>
            {
                player.OnCommand += (command, arguments) =>
                {
                    if(command == "say")
                    {
                        string message = $"{player.Name}: {string.Join(' ', arguments)}";
                        var packet = new ChatEchoPacket(player.Id, message, Color.White);
                        foreach (var _player in elementRepository.GetByType<Player>(ElementType.Player))
                        {
                            _player.Client.SendPacket(packet);
                            logger?.LogInformation(message);
                        }
                    }
                };
            };
        }
    }
}
