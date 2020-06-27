using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MtaServer.Packets.Definitions.Commands;
using MtaServer.Server.Elements;

namespace MtaServer.Server.Behaviour
{
    public class DefaultChatBehaviour
    {
        public DefaultChatBehaviour(MtaServer server)
        {
            Client.OnJoin += (client) =>
            {
                client.OnCommand += (command, arguments) =>
                {
                    if(command == "say")
                    {
                        var packet = new ChatEchoPacket(server.Root.Id, client.Name + ": " + string.Join(' ', arguments), Color.White);
                        foreach (var player in server.ElementRepository.GetByType<Client>(ElementType.Player))
                        {
                            player.SendPacket(packet);
                        }
                    }
                };
            };
        }
    }
}
