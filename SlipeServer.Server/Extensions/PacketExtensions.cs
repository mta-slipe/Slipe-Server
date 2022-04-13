using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using System.Collections.Generic;

namespace SlipeServer.Server.Extensions;

public static class PacketExtensions
{
    public static void SendTo(this Packet packet, IClient client) => client.SendPacket(packet);
    public static void SendTo(this Packet packet, Player player) => player.Client.SendPacket(packet);

    public static void SendTo(this Packet packet, IEnumerable<IClient> clients)
    {
        byte[] data = packet.Write();
        foreach (var client in clients)
        {
            client.SendPacket(packet.PacketId, data, packet.Priority, packet.Reliability);
        }
    }

    public static void SendTo(this Packet packet, IEnumerable<Player> players)
    {
        byte[] data = packet.Write();
        foreach (var player in players)
        {
            player.Client.SendPacket(packet.PacketId, data, packet.Priority, packet.Reliability);
        }
    }
}
