using SlipeServer.Net.Wrappers;
using SlipeServer.Packets;
using SlipeServer.Packets.Constants;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling;
using System;
using System.Net;

namespace SlipeServer.Server.Clients;

/// <summary>
/// Representation of a client connected to the server
/// </summary>
/// <typeparam name="TPlayer"></typeparam>
public class Client<TPlayer>
    : IClient, IClient<TPlayer>
    where TPlayer : Player
{
    private readonly INetWrapper netWrapper;
    private readonly ulong binaryAddress;
    private ushort bitStreamVersion;

    protected TPlayer Player { get; set; }
    TPlayer IClient<TPlayer>.Player
    {
        get => this.Player;
        set => this.Player = value;
    }


    /// <summary>
    /// The player that this client is associated to
    /// </summary>
    Player IClient.Player
    {
        get => ((IClient<TPlayer>)this).Player;
        set => ((IClient<TPlayer>)this).Player = (TPlayer)value;
    }

    /// <summary>
    /// Player serial, this can be null early on in the connection process
    /// </summary>
    public string? Serial { get; private set; }

    /// <summary>
    /// Player's extra data (data received durring connection), this can be null early on in the connection process
    /// </summary>
    public string? Extra { get; private set; }

    /// <summary>
    /// Player's MTA version, this can be null early on in the connection process
    /// </summary>
    public string? Version { get; private set; }

    /// <summary>
    /// Players IP Address, this can be null early on in the connection process
    /// </summary>
    public IPAddress? IPAddress { get; set; }

    /// <summary>
    /// Indicates whether or not the client is currently connected
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// The client's current connection state, indicating where in the connection process the client currently is
    /// </summary>
    public ClientConnectionState ConnectionState { get; protected set; }

    /// <summary>
    /// The client's most recent ping
    /// </summary>
    public uint Ping { get; set; }

    /// <summary>
    /// Creates a client
    /// </summary>
    /// <param name="binaryAddress">The identifier using within the networking interface for the client</param>
    /// <param name="netWrapper">The networking interface the client is connected to</param>
    /// <param name="player">The player this client is associated with</param>
    public Client(ulong binaryAddress, INetWrapper netWrapper, TPlayer player)
    {
        this.binaryAddress = binaryAddress;
        this.netWrapper = netWrapper;
        this.Player = player;
        this.IsConnected = true;
    }

    /// <summary>
    /// Sends a single packet to the client
    /// </summary>
    /// <param name="packet"></param>
    public void SendPacket(Packet packet)
    {
        if (!CanSendPacket(packet.PacketId))
            return;

        this.netWrapper.SendPacket(this.binaryAddress, this.bitStreamVersion, packet);
        HandleSentPacket(packet.PacketId);
    }


    /// <summary>
    /// Sends a single packet to the client
    /// </summary>
    /// <param name="packetId"></param>
    public void SendPacket(PacketId packetId, byte[] data, PacketPriority priority = PacketPriority.Medium, PacketReliability reliability = PacketReliability.Unreliable)
    {
        if (!CanSendPacket(packetId))
            return;

        this.netWrapper.SendPacket(this.binaryAddress, packetId, this.bitStreamVersion, data, priority, reliability);
        HandleSentPacket(packetId);
    }

    private bool CanSendPacket(PacketId packet)
    {
        if (packet == PacketId.PACKET_ID_SERVER_DISCONNECTED)
            return true;

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

    /// <summary>
    /// Sets a client's connection state to be Quit
    /// </summary>
    public void SetDisconnected()
    {
        this.ConnectionState = ClientConnectionState.Quit;
    }

    /// <summary>
    /// Resets a client's connection state, allowing the client to be re-used for a new connection
    /// </summary>
    public void ResetConnectionState()
    {
        this.ConnectionState = ClientConnectionState.Disconnected;
    }

    /// <summary>
    /// Sets the client's bitstream version
    /// </summary>
    /// <param name="version"></param>
    public void SetVersion(ushort version)
    {
        this.bitStreamVersion = version;
        if (this.IsConnected)
            this.netWrapper.SetVersion(this.binaryAddress, version);
    }

    /// <summary>
    /// Requests the client to re-send mod information packets
    /// </summary>
    public void ResendModPackets()
    {
        if (this.IsConnected)
            this.netWrapper.ResendModPackets(this.binaryAddress);
    }

    /// <summary>
    /// Requests the client to re-send anti cheat information packets.
    /// </summary>
    public void ResendPlayerACInfo()
    {
        if (this.IsConnected)
            this.netWrapper.ResendPlayerACInfo(this.binaryAddress);
    }

    /// <summary>
    /// Fetches the client's serial and MTA version
    /// </summary>
    public void FetchSerial()
    {
        var serialExtraAndVersion = this.netWrapper.GetClientSerialExtraAndVersion(this.binaryAddress);
        this.Serial = serialExtraAndVersion.Item1;
        this.Extra = serialExtraAndVersion.Item2;
        this.Version = serialExtraAndVersion.Item3;
    }

    /// <summary>
    /// Fetches the client's IP address
    /// </summary>
    public void FetchIp()
    {
        this.IPAddress = this.netWrapper.GetPlayerIp(this.binaryAddress);
    }
}

/// <summary>
/// Representation of a client connected to the server
/// </summary>
public class Client : Client<Player>
{
    public Client(ulong binaryAddress, INetWrapper netWrapper, Player player)
        : base(binaryAddress, netWrapper, player)
    {

    }
}
