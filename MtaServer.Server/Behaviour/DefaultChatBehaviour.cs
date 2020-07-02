using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MtaServer.Packets.Definitions.Commands;
using MtaServer.Server.Elements;
using MtaServer.Server.Repositories;

namespace MtaServer.Server.Behaviour
{
    public class DefaultChatBehaviour
    {
        public DefaultChatBehaviour(IElementRepository elementRepository)
        {
            Player.OnJoin += (player) =>
            {
                player.OnCommand += (command, arguments) =>
                {
                    if(command == "say")
                    {
                        var packet = new ChatEchoPacket(player.Id, player.Name + ": " + string.Join(' ', arguments), Color.White);
                        foreach (var _player in elementRepository.GetByType<Player>(ElementType.Player))
                        {
                            _player.Client.SendPacket(packet);
                        }
                    }
                };
            };
        }
    }
}
