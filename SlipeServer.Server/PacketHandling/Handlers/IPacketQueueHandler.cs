using SlipeServer.Packets;

namespace SlipeServer.Server.PacketHandling.Handlers
{
    public interface IPacketQueueHandler<T> where T : Packet
    {
        void EnqueuePacket(Client client, T packet);
    }
}
