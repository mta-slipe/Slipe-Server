using SlipeServer.Net.Wrappers.Enums;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Net;

namespace SlipeServer.Net.Wrappers;

public interface INetWrapper
{
    event Action<INetWrapper, ulong, PacketId, byte[], uint?>? PacketReceived;

    Tuple<string, string, string> GetClientSerialExtraAndVersion(ulong binaryAddress);
    IPAddress GetPlayerIp(ulong binaryAddress);
    void SendPacket(ulong binaryAddress, ushort bitStreamVersion, Packet packet);
    void SendPacket(ulong binaryAddress, PacketId packetId, ushort bitStreamVersion, byte[] data, PacketPriority priority = PacketPriority.High, PacketReliability reliability = PacketReliability.ReliableSequenced);
    void SetVersion(ulong binaryAddress, ushort version);
    void ResendModPackets(ulong binaryAddress);
    void ResendPlayerACInfo(ulong binaryAddress);
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
