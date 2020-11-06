using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MtaServer.Server.Elements;
using MtaServer.Server.PacketHandling.Factories;
using MtaServer.Server.Repositories;

namespace MtaServer.Server.Behaviour
{
    /// <summary>
    /// Behaviour responsible for sending create entity packets for all elements in the repository on player join 
    /// </summary>
    public class PlayerJoinElementBehaviour
    {
        private readonly IElementRepository elementRepository;

        public PlayerJoinElementBehaviour(IElementRepository elementRepository, MtaServer server)
        {
            this.elementRepository = elementRepository;

            server.PlayerJoined += OnPlayerJoin;
        }

        private void OnPlayerJoin(Player player)
        {
            var elements = this.elementRepository.GetAll();
            var packet = AddEntityPacketFactory.CreateAddEntityPacket(elements);
            player.Client.SendPacket(packet);
        }
    }
}
