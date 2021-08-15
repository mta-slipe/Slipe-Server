using SlipeServer.Net.Enums;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;

namespace SlipeServer.Net
{
    public interface INetWrapper
    {
        event Action<NetWrapper, uint, PacketId, byte[], uint?>? PacketReceived;

        Tuple<string, string, string> GetClientSerialExtraAndVersion(uint binaryAddress);
        void SendPacket(uint binaryAddress, Packet packet);
        void SendPacket(uint binaryAddress, PacketId packetId, byte[] data, PacketPriority priority = PacketPriority.High, PacketReliability reliability = PacketReliability.ReliableSequenced);
        void SetVersion(uint binaryAddress, ushort version);
        void ResendModPackets(uint binaryAddress);
        void ResendPlayerACInfo(uint binaryAddress);
        void SetAntiCheatConfig(
            IEnumerable<AntiCheat> disabledAntiCheats,
            bool hideAntiCheatFromClient,
            AllowGta3ImgMods allowGta3ImgMods,
            IEnumerable<SpecialDetection> enabledSpecialDetections,
            DataFile disallowedDataFiles
        );

        void Start();
        void Stop();
    }
}