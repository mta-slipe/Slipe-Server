using SlipeServer.Packets;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.PacketHandling.Handlers
{
    public interface IPacketHandler<T> where T : Packet
    {
        public PacketId PacketId { get; }

        void HandlePacket(Client client, T packet);
    }
}
