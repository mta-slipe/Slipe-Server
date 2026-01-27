using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.PacketHandling.Handlers;

namespace SlipeServer.Server.PacketHandling.Reducing;

public interface IPacketReducer
{
    void EnqueuePacket(IClient client, PacketId packetId, byte[] data);
    void RegisterPacketHandler<TPacket>(PacketId packetId, IPacketQueueHandler<TPacket> handler) where TPacket : Packet, new();
    void RemovePacketHandler<TPacket>(PacketId packetId, IPacketQueueHandler<TPacket> handler) where TPacket : Packet, new();
}
