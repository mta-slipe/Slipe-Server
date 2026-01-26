using SlipeServer.Console.Proxy;
using SlipeServer.Net.Wrappers;
using SlipeServer.Net.Wrappers.Enums;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using System.Net;

namespace SlipeServer.Server.Proxy;

public class ProxyNetWrapper : INetWrapper
{
    private readonly ProxyService proxyService;

    public ProxyNetWrapper(ProxyService proxyService)
    {
        this.proxyService = proxyService;
    }

    public void Start() { }

    public void Stop() { }

    protected virtual void SendPacket(ulong binaryAddress, byte packetId, ushort bitStreamVersion, byte[] payload, PacketPriority priority, PacketReliability reliability)
    {
        this.proxyService.SendMessage(RemoteMessageType.packet, packetId, binaryAddress, payload);
    }

    public void SendPacket(ulong binaryAddress, ushort bitStreamVersion, Packet packet)
    {
        SendPacket(binaryAddress, (byte)packet.PacketId, bitStreamVersion, packet.Write(), packet.Priority, packet.Reliability);
    }

    public void SendPacket(ulong binaryAddress, PacketId packetId, ushort bitStreamVersion, byte[] data, PacketPriority priority = PacketPriority.High, PacketReliability reliability = PacketReliability.ReliableSequenced)
    {
        SendPacket(binaryAddress, (byte)packetId, bitStreamVersion, data, priority, reliability);
    }

    public Tuple<string, string, string> GetClientSerialExtraAndVersion(ulong binaryAddress)
    {
        return new Tuple<string, string, string>(
            Guid.NewGuid().ToString(),
            binaryAddress.ToString(),
            "1.6.0-001"
        );
    }

    public IPAddress GetPlayerIp(ulong binaryAddress) => IPAddress.Any;

    public void ResendModPackets(ulong binaryAddress) { }

    public void ResendPlayerACInfo(ulong binaryAddress) { }

    public void SetVersion(ulong binaryAddress, ushort version) { }

    public void SetAntiCheatConfig(
        IEnumerable<AntiCheat> disabledAntiCheats,
        bool hideAntiCheatFromClient,
        AllowGta3ImgMods allowGta3ImgMods,
        IEnumerable<SpecialDetection> enabledSpecialDetections,
        DataFile disallowedDataFiles
    )
    { }

    public void TriggerPacketReceival(uint id, PacketId packet, byte[] payload)
    {
        this.PacketReceived?.Invoke(this, id, packet, payload, null);
    }

    public event Action<INetWrapper, ulong, PacketId, byte[], uint?>? PacketReceived;
}
