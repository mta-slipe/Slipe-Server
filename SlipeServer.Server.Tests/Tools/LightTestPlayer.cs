using SlipeServer.Net.Wrappers;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.Tests.Tools;

public class LightTestPlayer : Player
{
    public ulong Address { get; }

    public LightTestPlayer(INetWrapper wrapper, ulong address)
    {
        this.Client = new LightTestClient(address, wrapper, this);
        this.Address = address;
    }

    public struct ClientSendPacketCall
    {
        public PacketId PacketId { get; set; }
        public byte[] Data { get; set; }
        public PacketReliability Reliability { get; set; }
        public PacketPriority Priority { get; set; }
    }
}
public static class PlayerExtensions
{
    public static ulong GetAddress(this Player player) => ((LightTestClient)player.Client).Address;
}
