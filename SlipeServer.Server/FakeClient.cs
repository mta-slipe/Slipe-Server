using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System;
using System.Net;

namespace SlipeServer.Server;

public class FakeClient : IClient
{
    public Player? Player { get; set; }

    public string? Serial { get; set; }

    public string? Extra { get; set; }

    public string? Version { get; set; }

    public IPAddress? IPAddress { get; set; }
    public bool IsConnected { get; set; }

    public ClientConnectionState ConnectionState { get; set; }

    public uint Ping { get; set; }

    public void FetchSerial() { }
    public void ResendModPackets() { }
    public void ResendPlayerACInfo() { }
    public void SendPacket(Packet packet) { }
    public void SendPacket(PacketId packetId, byte[] data, PacketPriority priority = PacketPriority.Medium, PacketReliability reliability = PacketReliability.Unreliable) { }
    public void SetVersion(ushort version) { }
    public void SetDisconnected() { }
    public void ResetConnectionState() { }
}
