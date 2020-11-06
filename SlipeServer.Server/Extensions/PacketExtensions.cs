using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipeServer.Server.Extensions
{
    public static class PacketExtensions
    {
        public static void SendTo(this Packet packet, Client client) => client.SendPacket(packet);
        public static void SendTo(this Packet packet, Player player) => player.Client.SendPacket(packet);

        public static void SendTo(this Packet packet, IEnumerable<Client> clients)
        {
            byte[] data = packet.Write();
            foreach(var client in clients)
            {
                client.SendPacket(packet.PacketId, data);
            }
        }

        public static void SendTo(this Packet packet, IEnumerable<Player> players)
        {
            SendTo(packet, players.Select(p => p.Client));
        }
    }
}
