using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Net
{
    public interface INetWrapper
    {
        event Action<NetWrapper, uint, PacketId, byte[]>? PacketReceived;

        Tuple<string, string, string> GetClientSerialExtraAndVersion(uint binaryAddress);
        void SendPacket(uint binaryAddress, Packet packet);
        void SendPacket(uint binaryAddress, PacketId packetId, byte[] data, PacketPriority priority = PacketPriority.High, PacketReliability reliability = PacketReliability.ReliableSequenced);
        void SetVersion(uint binaryAddress, ushort version);
        void Start();
        void Stop();
    }
}