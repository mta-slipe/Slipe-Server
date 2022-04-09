using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Constants;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling;
using System;
using System.Net;

namespace SlipeServer.Server;

public class Client
{
    private readonly INetWrapper netWrapper;
    private readonly uint binaryAddress;
    private ushort bitStreamVersion;

    public Player Player { get; protected set; }

    public string? Serial { get; private set; }
    public string? Extra { get; private set; }
    public string? Version { get; private set; }
    public IPAddress? IPAddress { get; set; }
    public bool IsConnected { get; internal set; }
    public ClientConnectionState ConnectionState { get; protected set; }
    public uint Ping { get; set; }

    public Client(uint binaryAddress, INetWrapper netWrapper)
    {
        this.binaryAddress = binaryAddress;
        this.netWrapper = netWrapper;
        this.Player = new Player(this);
        this.IsConnected = true;
    }

    public void SendPacket(Packet packet)
    {
        if (CanSendPacket(packet.PacketId))
        {
            this.netWrapper.SendPacket(this.binaryAddress, this.bitStreamVersion, packet);
            HandleSentPacket(packet.PacketId);
        }
    }

    public void SendPacket(PacketId packetId, byte[] data, PacketPriority priority = PacketPriority.Medium, PacketReliability reliability = PacketReliability.Unreliable)
    {
        if (CanSendPacket(packetId))
        {
            this.netWrapper.SendPacket(this.binaryAddress, packetId, this.bitStreamVersion, data, priority, reliability);
            HandleSentPacket(packetId);
        }
    }

    private bool CanSendPacket(PacketId packet)
    {
        return 
            this.IsConnected && 
            (
                ClientPacketScope.Current == null || 
                ClientPacketScope.Current.ContainsClient(this)
            ) &&
            (
                this.ConnectionState == ClientConnectionState.Joined || 
                PacketSendingConstants.AlwaysAllowedPackets.Contains(packet)
            );
    }

    private void HandleSentPacket(PacketId packet)
    {
        if (Enum.IsDefined((ClientConnectionState)packet))
            this.ConnectionState = (ClientConnectionState)packet;
    }

    public void SetVersion(ushort version)
    {
        this.bitStreamVersion = version;
        if (this.IsConnected)
        {
            this.netWrapper.SetVersion(this.binaryAddress, version);
        }
    }

    public void ResendModPackets()
    {
        if (this.IsConnected)
        {
            this.netWrapper.ResendModPackets(this.binaryAddress);
        }
    }

    public void ResendPlayerACInfo()
    {
        if (this.IsConnected)
        {
            this.netWrapper.ResendPlayerACInfo(this.binaryAddress);
        }
    }

    public void FetchSerial()
    {
        Tuple<string, string, string> serialExtraAndVersion = this.netWrapper.GetClientSerialExtraAndVersion(this.binaryAddress);
        this.Serial = serialExtraAndVersion.Item1;
        this.Extra = serialExtraAndVersion.Item2;
        this.Version = serialExtraAndVersion.Item3;
    }
}
