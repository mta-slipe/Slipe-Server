using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Net;

namespace SlipeServer.Server;

public interface IClient
{
    public Player Player { get; set; }

    public string? Serial { get; }
    public string? Extra { get; }
    public string? Version { get; }
    public IPAddress? IPAddress { get; set; }
    public bool IsConnected { get; set; }
    public ClientConnectionState ConnectionState { get; }
    public uint Ping { get; set; }

    public void SendPacket(Packet packet);

    public void SendPacket(PacketId packetId, byte[] data, PacketPriority priority = PacketPriority.Medium, PacketReliability reliability = PacketReliability.Unreliable);

    public void SetVersion(ushort version);

    public void ResendModPackets();

    public void ResendPlayerACInfo();

    public void FetchSerial();

    public void SetDisconnected();

    public void ResetConnectionState();
}

public interface IClient<TPlayer> : IClient where TPlayer : Player
{
    new TPlayer Player { get; set; }
}

