using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.PacketHandling.Handlers;

public interface IPacketHandler<T> where T : Packet
{
    public PacketId PacketId { get; }

    void HandlePacket(IClient client, T packet);
}
