using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Net;

namespace SlipeServer.Server.Clients;

public interface IClient
{
    public Player Player { get; set; }

    /// <summary>
    /// Network wrapper serving this client, or null for clients without a native wrapper.
    /// Custom clients participating in verified ASE counts should expose their wrapper.
    /// </summary>
    INetWrapper? NetWrapper => null;

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
    public void FetchIp();

    public void SetDisconnected();

    public void ResetConnectionState();
}

public interface IClient<TPlayer> : IClient where TPlayer : Player
{
    new TPlayer Player { get; set; }
}

