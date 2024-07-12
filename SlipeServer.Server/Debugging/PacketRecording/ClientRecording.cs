using SlipeServer.Packets.Enums;
using SlipeServer.Packets;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Net;

namespace SlipeServer.Server.Debugging.PacketRecording;

public delegate void PacketSent(Packet packet);

public class ClientRecording : IClient
{
    public event PacketSent? PacketSent;
    private readonly IClient client;

    public IClient Client => this.client;
    public Player Player { get; set; }
    public string? Serial => this.client.Serial;
    public string? Extra => this.client.Extra;
    public string? Version => this.client.Version;
    public IPAddress? IPAddress { get => this.client.IPAddress; set => this.client.IPAddress = value; }
    public bool IsConnected { get => this.client.IsConnected; set => this.client.IsConnected = value; }
    public ClientConnectionState ConnectionState => this.client.ConnectionState;
    public uint Ping { get => this.client.Ping; set => this.client.Ping = value; }

    public ClientRecording(Player player)
    {
        this.client = player.Client;
        player.Client = this;
        this.Player = player;
    }

    public void FetchIp() => this.client.FetchIp();
    public void FetchSerial() => this.client.FetchSerial();
    public void ResendModPackets() => this.client.ResendModPackets();
    public void ResendPlayerACInfo() => this.client?.ResendPlayerACInfo();
    public void ResetConnectionState() => this.client.ResetConnectionState();
    public void SendPacket(Packet packet)
    {
        PacketSent?.Invoke(packet);
        this.client.SendPacket(packet);
    }

    public void SendPacket(PacketId packetId, byte[] data, PacketPriority priority = PacketPriority.Medium, PacketReliability reliability = PacketReliability.Unreliable)
    {
        //PacketSent?.Invoke(packetId, data, priority, reliability);
        this.client.SendPacket(packetId, data, priority, reliability);
    }

    public void SetDisconnected() => this.client.SetDisconnected();
    public void SetVersion(ushort version) => this.client.SetVersion(version);
}

