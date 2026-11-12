using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Weather;

public sealed class SetWeatherBlendedRpcPacket(byte weatherId, byte currentHour) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte CurrentHour { get; } = currentHour;
    public byte WeatherId { get; } = weatherId;
    
    public override void Read(byte[] bytes) => throw new System.NotImplementedException();

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_WEATHER_BLENDED);
        builder.Write(this.WeatherId);
        builder.Write((byte)(this.CurrentHour+1));

        return builder.Build();
    } 
}
