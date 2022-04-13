using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System;
using System.Net;

namespace SlipeServer.Server;

public class NullClient : IClient
{
    public Player Player
    {
        get => throw new NullReferenceException("Attempt to access client of player without client.");
        set => throw new NullReferenceException("Attempt to access client of player without client.");
    }

    public string? Serial => throw new NullReferenceException("Attempt to access client of player without client.");

    public string? Extra => throw new NullReferenceException("Attempt to access client of player without client.");

    public string? Version => throw new NullReferenceException("Attempt to access client of player without client.");

    public IPAddress? IPAddress { get => throw new NullReferenceException("Attempt to access client of player without client."); set => throw new NullReferenceException("Attempt to access client of player without client."); }
    public bool IsConnected { get => throw new NullReferenceException("Attempt to access client of player without client."); set => throw new NullReferenceException("Attempt to access client of player without client."); }

    public ClientConnectionState ConnectionState => throw new NullReferenceException("Attempt to access client of player without client.");

    public uint Ping { get => throw new NullReferenceException("Attempt to access client of player without client."); set => throw new NullReferenceException("Attempt to access client of player without client."); }

    public void FetchSerial() => throw new NullReferenceException("Attempt to access client of player without client.");
    public void ResendModPackets() => throw new NullReferenceException("Attempt to access client of player without client.");
    public void ResendPlayerACInfo() => throw new NullReferenceException("Attempt to access client of player without client.");
    public void SendPacket(Packet packet) => throw new NullReferenceException("Attempt to access client of player without client.");
    public void SendPacket(PacketId packetId, byte[] data, PacketPriority priority = PacketPriority.Medium, PacketReliability reliability = PacketReliability.Unreliable) => throw new NullReferenceException("Attempt to access client of player without client.");
    public void SetVersion(ushort version) => throw new NullReferenceException("Attempt to access client of player without client.");

    private void Throw() => throw new NullReferenceException("Attempt to access client of player without client.");
}
